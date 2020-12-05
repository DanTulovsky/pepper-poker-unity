﻿using System;
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
    private long playerPosition;
    private long lastTurnID = -1;
    private string lastAckToken = "";
    private Player player;
    
    public readonly ClientInfo ClientInfo = new ClientInfo();
    private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

    public readonly GameData GameData = new GameData();

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

        ui = GameObject.Find("UI").GetComponent<UI>();
        Assert.IsNotNull(ui);
        
        ui.playerUsernameInput.text = "dant";
        ui.playerPasswordInput.text = "password";
    }

    public void Register()
    {

        ClientInfo.PlayerUsername = ui.playerUsernameInput.text;
        ClientInfo.Password = ui.playerPasswordInput.text;

        try
        {
            ClientInfo.PlayerID = pokerClient.Register(ClientInfo);
        }
        catch (RpcException)
        {
           return; 
        }

        ui.playerUsernameDisplay.SetText(ClientInfo.PlayerUsername);

        JoinTable();
    }

    private void JoinTable()
    {
        Debug.Log("Joining table...");

        string id;
        long position;
        
        try
        {
            (id, position) = pokerClient.JoinTable(ClientInfo);
        }
        catch (RpcException)
        {
            return;
        }

        ClientInfo.TableID = id;
        playerPosition = position;

        Debug.Log("Table ID: " + ClientInfo.TableID);
        Debug.Log("Player Position: " + playerPosition);

        // Kick off background refresh thread for GameData
        StartInfoStream();
    }

    private Player Player(string id)
    {
        player = player ?? GameData.PlayerFromID(id);
        return player;
    }

    public void ActionAllIn()
    {
        if (!GameData.IsMyTurn(ClientInfo.PlayerID, lastTurnID)) { return; }

        long amount = Player(ClientInfo.PlayerID).Money.Stack;
        try
        {
            pokerClient.ActionBet(ClientInfo, amount);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }
        lastTurnID = GameData.WaitTurnNum();
    }

    public void ActionCheck()
    {
        if (!GameData.IsMyTurn(ClientInfo.PlayerID, lastTurnID)) { return; }

        try
        {
            pokerClient.ActionCheck(ClientInfo);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }
        lastTurnID = GameData.WaitTurnNum();
    }

    public void ActionCall()
    {
        if (!GameData.IsMyTurn(ClientInfo.PlayerID, lastTurnID)) { return; }

        try
        {
            pokerClient.ActionCall(ClientInfo);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }
        lastTurnID = GameData.WaitTurnNum();
    }

    public void ActionFold()
    {
        if (!GameData.IsMyTurn(ClientInfo.PlayerID, lastTurnID)) { return; }

        try
        {
            pokerClient.ActionFold(ClientInfo);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }
        lastTurnID = GameData.WaitTurnNum();
    }

    public void ActionBet()
    {
        if (!GameData.IsMyTurn(ClientInfo.PlayerID, lastTurnID)) { return; }
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
            pokerClient.ActionBet(ClientInfo, amount);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex);
            return;
        }
        lastTurnID = GameData.WaitTurnNum();
    }

    private void StartInfoStream()
    {
        stream = pokerClient.GetGameDataStreaming(ClientInfo);

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
                tokenSource.Token.ThrowIfCancellationRequested();

                Poker.GameData gd = stream.ResponseStream.Current;
                GameData.Set(gd);

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
           pokerClient.ActionAckToken(ClientInfo, token);
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
