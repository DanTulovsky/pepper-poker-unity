using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Poker;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public UI ui;

    private PokerClient pokerClient;
    private long playerPosition;
    private long lastTurnID = -1;
    private Player player;
    
    private ClientInfo clientInfo = new ClientInfo();

    private readonly GameData gameData = new GameData();

    // Post-round start cancellation token
    private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

    // server streaming for GameData from server
    private AsyncServerStreamingCall<Poker.GameData> stream;

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
    }

    public void Register()
    {

        clientInfo.PlayerUsername = ui.playerUsernameInput.text;
        clientInfo.Password = ui.playerPasswordInput.text;
        clientInfo.PlayerID = pokerClient.Register(clientInfo);

        ui.playerUsernameDisplay.SetText(clientInfo.PlayerUsername);

        JoinTable();
    }

    private void JoinTable()
    {
        Debug.Log("Joining table...");
        (string id, long position) = pokerClient.JoinTable(clientInfo);
        clientInfo.TableID = id;
        playerPosition = position;

        Debug.Log("Table ID: " + clientInfo.TableID);
        Debug.Log("Player Position: " + playerPosition);

        // Kick off background refresh thread for gameData
        StartInfoStream();
    }

    private Player Player(string id)
    {
        player = player ?? gameData.PlayerFromID(id);
        return player;
    }

    public void ActionAllIn()
    {
        if (!gameData.IsMyTurn(clientInfo.PlayerID, lastTurnID)) { return; }

        long amount = Player(clientInfo.PlayerID).Money.Stack;
        try
        {
            pokerClient.ActionBet(clientInfo, amount);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }
        lastTurnID = gameData.WaitTurnNum();
    }

    public void ActionCheck()
    {
        if (!gameData.IsMyTurn(clientInfo.PlayerID, lastTurnID)) { return; }

        try
        {
            pokerClient.ActionCheck(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }
        lastTurnID = gameData.WaitTurnNum();
    }

    public void ActionCall()
    {
        if (!gameData.IsMyTurn(clientInfo.PlayerID, lastTurnID)) { return; }

        try
        {
            pokerClient.ActionCall(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }
        lastTurnID = gameData.WaitTurnNum();
    }

    public void ActionFold()
    {
        if (!gameData.IsMyTurn(clientInfo.PlayerID, lastTurnID)) { return; }

        try
        {
            pokerClient.ActionFold(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }
        lastTurnID = gameData.WaitTurnNum();
    }

    public void ActionBet()
    {
        if (!gameData.IsMyTurn(clientInfo.PlayerID, lastTurnID)) { return; }
        long amount;
        string input = ui.betAmount.text;
        try
        {
            amount = Convert.ToInt64(input);
        }
        catch (FormatException ex)
        {
            Debug.Log(ex.ToString());
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
        lastTurnID = gameData.WaitTurnNum();
    }

    private void StartInfoStream()
    {
        stream = pokerClient.GetGameDataStreaming(clientInfo);

        Debug.Log("Starting server stream listener...");
        StartCoroutine(nameof(StartServerStream));

    }


    // StartServerStream starts a background task listening to server responses
    // https://github.com/grpc/grpc/issues/21734#issuecomment-578519701
    private async Task<AsyncServerStreamingCall<Poker.GameData>> StartServerStream()
    {
        try
        {
            while (await stream.ResponseStream.MoveNext())
            {
                Debug.Log("got data");
                // Exit if application is stopped
                tokenSource.Token.ThrowIfCancellationRequested();

                Poker.GameData gd = stream.ResponseStream.Current;
                gameData.Set(gd);

                Debug.Log($"> {gd}");
                ui.UpdateUI(gameData, clientInfo.PlayerID);
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
        tokenSource.Cancel();
        return null;
    }


    // Update is called once per frame
    private void Update()
    {

    }

    private void OnApplicationQuit()
    {
        tokenSource.Cancel();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }
}
