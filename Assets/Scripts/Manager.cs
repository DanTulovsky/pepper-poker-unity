using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Poker;
using UnityEngine;
using UnityEngine.Assertions;

public class Manager : Singleton<Manager>
{
    private UI ui;
    private PokerClient pokerClient;
    private long lastTurnID = -1;
    private string lastAckToken = "";

    public readonly ClientInfo clientInfo = new ClientInfo();
    private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

    public readonly Game Game = new Game();

    // server streaming for Game from server
    private AsyncServerStreamingCall<GameData> stream;
    //private Grpc.Core.Logging.LogLevelFilterLogger logger;

    // Start is called before the first frame update
    private void Start()
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

        ui = GameObject.Find("UI").GetComponent<UI>();
        Assert.IsNotNull(ui);

        ui.playerUsernameInput.text = "dant";
        ui.playerPasswordInput.text = "password";
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

        clientInfo.PlayerUsername = ui.playerUsernameInput.text;
        clientInfo.Password = ui.playerPasswordInput.text;

        try
        {
            clientInfo.PlayerID = pokerClient.Register(clientInfo);
        }
        catch (RpcException)
        {
            return;
        }

        ui.playerUsernameDisplay.SetText(clientInfo.PlayerUsername);
    }

    private void DoJoinTable()
    {
        Debug.Log("Joining table...");

        try
        {
            (clientInfo.TableID, Game.PlayerPosition) = pokerClient.JoinTable(clientInfo);
        }
        catch (RpcException)
        {
            return;
        }

        Debug.Log("Table ID: " + clientInfo.TableID);
        Debug.Log("Player Position: " + Game.PlayerPosition);

        // Kick off background refresh thread for Game
        StartInfoStream();
    }

    public void ActionAllIn()
    {
        if (!Game.IsMyTurn(clientInfo.PlayerID, lastTurnID)) { return; }

        try
        {
            pokerClient.ActionAllIn(clientInfo);
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
            pokerClient.ActionBuyIn(clientInfo);
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
        if (!Game.IsMyTurn(clientInfo.PlayerID, lastTurnID)) { return; }

        try
        {
            pokerClient.ActionCheck(clientInfo);
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
        if (!Game.IsMyTurn(clientInfo.PlayerID, lastTurnID)) { return; }

        try
        {
            pokerClient.ActionCall(clientInfo);
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
        if (!Game.IsMyTurn(clientInfo.PlayerID, lastTurnID)) { return; }

        try
        {
            pokerClient.ActionFold(clientInfo);
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
        if (!Game.IsMyTurn(clientInfo.PlayerID, lastTurnID)) { return; }
        long amount;
        string input = ui.betAmountInput.text;
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
            pokerClient.ActionBet(clientInfo, amount);
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
        stream = pokerClient.GetGameDataStreaming(clientInfo);

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
                Game.PlayerPosition = gd.Player.Position;

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
            pokerClient.ActionAckToken(clientInfo, token);
        }
        catch (RpcException ex)
        {
            Debug.Log(ex.ToString());
        }

        lastAckToken = token;
    }

    private void OnApplicationQuit()
    {
        tokenSource.Cancel();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }
}
