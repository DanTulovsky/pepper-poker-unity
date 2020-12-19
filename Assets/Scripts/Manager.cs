using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Poker;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class RpcErrorEvent : UnityEvent<string> { }
public class GameFailedEvent : UnityEvent<string> { }

public class Manager : Singleton<Manager>
{
    public UI uiUpdater;
    private PokerClient pokerClient;
    private long lastTurnID = -1;
    private string lastAckToken = "";

    [NonSerialized] public readonly ClientInfo clientInfo = new ClientInfo();
    private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

    [NonSerialized] public readonly Game game = new Game();

    public List<TablePosition> tablePositions;

    // server streaming for Game from server
    private AsyncServerStreamingCall<GameData> stream;
    //private Grpc.Core.Logging.LogLevelFilterLogger logger;

    // On RPC error call these actions
    // private UnityAction rpcErrorAction;
    private RpcErrorEvent mRPCErrorEvent;
    private GameFailedEvent mGameFailedEvent;
    
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

    private void Start()
    {
        mRPCErrorEvent ??= new RpcErrorEvent();
        // TODO: Fix for all players
        mRPCErrorEvent.AddListener(tablePositions[3].avatar.AnimateShrug);
        
        mGameFailedEvent ??= new GameFailedEvent();
        mGameFailedEvent.AddListener(tablePositions[3].avatar.AnimateDefeat);
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
        clientInfo.PlayerUsername = uiUpdater.playerUsernameInput.text;
        clientInfo.Password = uiUpdater.playerPasswordInput.text;

        try
        {
            clientInfo.PlayerID = pokerClient.Register(clientInfo);
        }
        catch (RpcException ex)
        {
            mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        uiUpdater.playerUsernameDisplay.SetText(clientInfo.PlayerUsername);
        StartCoroutine(tablePositions[3].avatar.Say($"Hi there {clientInfo.PlayerUsername}!"));
        tablePositions[3].avatar.AnimateWave();
    }

    private void DoJoinTable()
    {
        Debug.Log("Joining table...");

        try
        {
            (clientInfo.TableID, game.PlayerRealPosition) = pokerClient.JoinTable(clientInfo);
        }
        catch (RpcException ex)
        {
            mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        Debug.Log("Table ID: " + clientInfo.TableID);
        Debug.Log("Player Position: " + game.PlayerRealPosition);

        // Kick off background refresh thread for Game
        StartInfoStream();
    }

    public void ActionAllIn()
    {
        if (!game.IsMyTurn(clientInfo.PlayerID, lastTurnID))
        {
            mRPCErrorEvent.Invoke("Not your turn!");
            return;
        }

        try
        {
            pokerClient.ActionAllIn(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
            mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        lastTurnID = game.WaitTurnNum();
    }

    public void ActionBuyIn()
    {
        try
        {
            pokerClient.ActionBuyIn(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
            mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        lastTurnID = game.WaitTurnNum();
    }

    public void ActionCheck()
    {
        if (!game.IsMyTurn(clientInfo.PlayerID, lastTurnID))
        {
            mRPCErrorEvent.Invoke("Not your turn!");
            return;
        }

        try
        {
            pokerClient.ActionCheck(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
            mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        lastTurnID = game.WaitTurnNum();
    }

    public void ActionCall()
    {
        if (!game.IsMyTurn(clientInfo.PlayerID, lastTurnID))
        {
            mRPCErrorEvent.Invoke("Not your turn!");
            return;
        }

        try
        {
            pokerClient.ActionCall(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
           mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        lastTurnID = game.WaitTurnNum();
    }

    public void ActionFold()
    {
        if (!game.IsMyTurn(clientInfo.PlayerID, lastTurnID))
        {
            mRPCErrorEvent.Invoke("Not your turn!");
            return;
        }

        try
        {
            pokerClient.ActionFold(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
            mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        lastTurnID = game.WaitTurnNum();
    }

    public void ActionBet()
    {
        if (!game.IsMyTurn(clientInfo.PlayerID, lastTurnID))
        {
            mRPCErrorEvent.Invoke("Not your turn!");
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
            mRPCErrorEvent.Invoke($"(input: {input}) {ex}");
            return;
        }

        try
        {
            pokerClient.ActionBet(clientInfo, amount);
        }
        catch (InvalidTurnException ex)
        {
            mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        lastTurnID = game.WaitTurnNum();
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
                tokenSource.Token.ThrowIfCancellationRequested();

                GameData gd = stream.ResponseStream.Current;
                game.Set(gd);
                game.PlayerRealPosition = gd.Player.Position;

                // Debug.Log($"> {gd}");

                AckIfNeeded(gd.Info.AckToken);
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            mGameFailedEvent.Invoke($"Stream cancelled: {ex}");
        }
        catch (OperationCanceledException ex)
        {
            mGameFailedEvent.Invoke($"server streaming thread cancelled: {ex}");
            stream.Dispose();
        }
        catch (RpcException ex)
        {
            mGameFailedEvent.Invoke(ex.ToString());
        }
        catch (Exception ex)
        {
            mGameFailedEvent.Invoke($"Server reading thread failed: {ex}");
            Application.Quit();
        }

        Debug.Log("Exiting server info thread...");
        return null;
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