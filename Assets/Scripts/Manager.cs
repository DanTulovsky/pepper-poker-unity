using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Poker;
using UnityEngine;

public class Manager : Singleton<Manager>
{
    public UI uiUpdater;
    private PokerClient pokerClient;
    private long lastTurnID = -1;
    private string lastAckToken = "";

    [NonSerialized] public readonly ClientInfo ClientInfo = new ClientInfo();
    private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

    [NonSerialized] public readonly Game Game = new Game();

    public List<TablePosition> tablePositions;

    // server streaming for Game from server
    private AsyncServerStreamingCall<GameData> stream;
    //private Grpc.Core.Logging.LogLevelFilterLogger logger;

    // Start is called before the first frame update
    public void Awake()
    {
        //Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "info");
        //Environment.SetEnvironmentVariable("GRPC_DNS_RESOLVER", "native");
        //Environment.SetEnvironmentVariable("GRPC_TRACE", "all");
        //Debug.Log("Setting Grpc Logger");

        //logger = new Grpc.Core.Logging.LogLevelFilterLogger(new GrpcLogger(), Grpc.Core.Logging.LogLevel.Info);
        //Grpc.Core.GrpcEnvironment.SetLogger(logger);

        //logger.Debug("GRPC_VERBOSITY = " + Environment.GetEnvironmentVariable("GRPC_VERBOSITY"));
        //logger.Debug("GRPC_TRACE = " + Environment.GetEnvironmentVariable("GRPC_TRACE"));

        pokerClient = new PokerClient();

        // UIUpdater = GameObject.Find("UI").GetComponent<UI>();
        // Assert.IsNotNull(UIUpdater);

        uiUpdater.playerUsernameInput.text = "dant";
        uiUpdater.playerPasswordInput.text = "password";

    }


    public void JoinTable()
    {
        StartCoroutine(nameof(DoJoinTable));
    }

    public void Register()
    {
        StartCoroutine(nameof(DoRegister));
    }

    private void DoRegister()
    {
        ClientInfo.PlayerUsername = uiUpdater.playerUsernameInput.text;
        ClientInfo.Password = uiUpdater.playerPasswordInput.text;

        try
        {
            ClientInfo.PlayerID = pokerClient.Register(ClientInfo);
        }
        catch (RpcException)
        {
            return;
        }

        uiUpdater.playerUsernameDisplay.SetText(ClientInfo.PlayerUsername);
    }

    private void DoJoinTable()
    {
        Debug.Log("Joining table...");

        try
        {
            (ClientInfo.TableID, Game.PlayerRealPosition) = pokerClient.JoinTable(ClientInfo);
        }
        catch (RpcException)
        {
            return;
        }

        Debug.Log("Table ID: " + ClientInfo.TableID);
        Debug.Log("Player Position: " + Game.PlayerRealPosition);

        // Kick off background refresh thread for Game
        StartInfoStream();
    }

    public void ActionAllIn()
    {
        if (!Game.IsMyTurn(ClientInfo.PlayerID, lastTurnID))
        {
            return;
        }

        try
        {
            pokerClient.ActionAllIn(ClientInfo);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }

        lastTurnID = Game.WaitTurnNum();
    }

    public void ActionBuyIn()
    {
        // if (!Game.IsMyTurn(ClientInfo.PlayerID, lastTurnID)) { return; }

        try
        {
            pokerClient.ActionBuyIn(ClientInfo);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }

        lastTurnID = Game.WaitTurnNum();
    }

    public void ActionCheck()
    {
        if (!Game.IsMyTurn(ClientInfo.PlayerID, lastTurnID))
        {
            return;
        }

        try
        {
            pokerClient.ActionCheck(ClientInfo);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }

        lastTurnID = Game.WaitTurnNum();
    }

    public void ActionCall()
    {
        if (!Game.IsMyTurn(ClientInfo.PlayerID, lastTurnID))
        {
            return;
        }

        try
        {
            pokerClient.ActionCall(ClientInfo);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }

        lastTurnID = Game.WaitTurnNum();
    }

    public void ActionFold()
    {
        if (!Game.IsMyTurn(ClientInfo.PlayerID, lastTurnID))
        {
            return;
        }

        try
        {
            pokerClient.ActionFold(ClientInfo);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }

        lastTurnID = Game.WaitTurnNum();
    }

    public void ActionBet()
    {
        if (!Game.IsMyTurn(ClientInfo.PlayerID, lastTurnID))
        {
            return;
        }

        long amount;
        string input = uiUpdater.betAmountInput.text;
        try
        {
            amount = Convert.ToInt64(input);
        }
        catch (FormatException ex)
        {
            Debug.Log($"({input}) {ex}");
            return;
        }

        try
        {
            pokerClient.ActionBet(ClientInfo, amount);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }

        lastTurnID = Game.WaitTurnNum();
    }

    private void StartInfoStream()
    {
        stream = pokerClient.GetGameDataStreaming(ClientInfo);

        Debug.Log("Starting server stream listener...");
        StartCoroutine(nameof(StartServerStream));
    }


    // StartServerStream starts a background task listening to server responses
    // https://github.com/grpc/grpc/issues/21734#issuecomment-578519701
    private async Task<AsyncServerStreamingCall<GameData>> StartServerStream()
    {
        try
        {
            while (await stream.ResponseStream.MoveNext())
            {
                Debug.Log("got data");
                tokenSource.Token.ThrowIfCancellationRequested();

                GameData gd = stream.ResponseStream.Current;
                Game.Set(gd);
                Game.PlayerRealPosition = gd.Player.Position;

                Debug.Log($"> {gd}");

                AckIfNeeded(gd.Info.AckToken);
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            Debug.Log("Stream cancelled");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("server streaming thread cancelled...");
            stream.Dispose();
        }
        catch (RpcException ex)
        {
            Debug.Log(ex.ToString());
        }
        catch (Exception ex)
        {
            Debug.Log($"Server reading thread failed: {ex}");
            Application.Quit();
        }

        Debug.Log("Exiting server info thread...");
        return null;
    }


    // Update is called once per frame
    private void Update()
    {
    }

    private void AckIfNeeded(string token)
    {
        if (lastAckToken == token || token == "")
        {
            return;
        }

        try
        {
            pokerClient.ActionAckToken(ClientInfo, token);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex.ToString());
        }

        lastAckToken = token;
    }

    private void OnApplicationQuit()
    {
        tokenSource.Cancel();
        StopAllCoroutines();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }
}