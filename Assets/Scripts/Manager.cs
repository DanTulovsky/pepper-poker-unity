using System;
using UnityEngine;
using Grpc.Core;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Poker;


public class Manager : MonoBehaviour {
    public UI ui;

    private PokerClient pokerClient;
    private string playerID;
    private string playerName;
    private long playerPosition;
    private long lastTurnID = -1;
    private Player player;
    private string tableID = "";
    private string roundID = "";

    private readonly TableInfo tableInfo = new TableInfo { };

    // Post-round start cancellation token
    private readonly System.Threading.CancellationTokenSource tokenSource = new System.Threading.CancellationTokenSource { };

    // bi-direction streaming for GetInfo from server
    private AsyncDuplexStreamingCall<GetInfoRequest, Poker.TableInfo> stream;

    //private Grpc.Core.Logging.LogLevelFilterLogger logger;


    // Start is called before the first frame update
    private void Start() {
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

    public void SayHello() {

        playerName = ui.playerNameInput.text;
        playerID = pokerClient.SayHello(playerName);

        ui.playerNameDisplay.SetText(playerName);

        StartCoroutine(nameof(JoinTable));
    }

    private IEnumerator JoinTable() {
        Debug.Log("Joining table...");
        var joinTableReturn = pokerClient.JoinTable(tableID, playerID);
        tableID = joinTableReturn.id;
        playerPosition = joinTableReturn.position;

        Debug.Log("Table ID: " + tableID.ToString());
        Debug.Log("Player Position: " + playerPosition.ToString());

        // Kick off background refresh thread for tableInfo
        StartInfoStream();

        // Wait for round to start
        Debug.Log("Waiting for round to start...");
        yield return new WaitUntil(() => tableInfo.RoundID() != "");
        roundID = tableInfo.RoundID();
        Debug.Log("> Round ID: " + roundID);
    }

    private Poker.Player Player(string playerID) {
        player = player ?? tableInfo.PlayerFromID(playerID);
        return player;
    }

    public void ActionAllIn() {
        if (!tableInfo.IsMyTurn(playerID, lastTurnID)) { return; }

        var amount = Player(playerID).Money.Stack;
        try {
            pokerClient.ActionBet(tableID, playerID, roundID, amount);
        } catch (InvalidTurnException ex) {
            Debug.Log(ex);
            return;
        }
        lastTurnID = tableInfo.TurnID();
    }

    public void ActionCheck() {
        if (!tableInfo.IsMyTurn(playerID, lastTurnID)) { return; }

        try {
            pokerClient.ActionCheck(tableID, playerID, roundID);
        } catch (InvalidTurnException ex) {
            Debug.Log(ex);
            return;
        }
        lastTurnID = tableInfo.TurnID();
    }

    public void ActionCall() {
        if (!tableInfo.IsMyTurn(playerID, lastTurnID)) { return; }

        try {
            pokerClient.ActionCall(tableID, playerID, roundID);
        } catch (InvalidTurnException ex) {
            Debug.Log(ex);
            return;
        }
        lastTurnID = tableInfo.TurnID();
    }

    public void ActionFold() {
        if (!tableInfo.IsMyTurn(playerID, lastTurnID)) { return; }

        try {
            pokerClient.ActionFold(tableID, playerID, roundID);
        } catch (InvalidTurnException ex) {
            Debug.Log(ex);
            return;
        }
        lastTurnID = tableInfo.TurnID();
    }

    public void ActionBet() {
        if (!tableInfo.IsMyTurn(playerID, lastTurnID)) { return; }
        long amount;
        var input = ui.betAmount.text;
        try {
            amount = Convert.ToInt64(input);
        } catch (FormatException ex) {
            Debug.Log(ex.ToString());
            return;
        }

        try {
            pokerClient.ActionBet(tableID, playerID, roundID, amount);
        } catch (InvalidTurnException ex) {
            Debug.Log(ex);
            return;
        }
        lastTurnID = tableInfo.TurnID();
    }

    private void StartInfoStream() {
        var call = pokerClient.GetInfoStreaming();
        this.stream = call;

        Debug.Log("Starting server stream listener...");
        StartCoroutine(nameof(StartServerStream));

        // Send client request
        StartCoroutine(nameof(StartClientStream));
    }

    // SendClientRequest sends a client request to the server
    private IEnumerator StartClientStream() {
        while (!tokenSource.IsCancellationRequested) {
            var req = new Poker.GetInfoRequest {
                PlayerID = playerID,
                TableID = tableID,
                RoundID = roundID,
            };
            Debug.Log("Sending client request...");
            Debug.Log("playerID: " + playerID);
            Debug.Log("tableID: " + tableID);
            Debug.Log("roundID: " + roundID);

            stream.RequestStream.WriteAsync(req);

            yield return new WaitForSeconds(2);
        }

        Debug.Log($"Exiting client info thread...");
    }

    // StartServerStream starts a background task listening to server responses
    // https://github.com/grpc/grpc/issues/21734#issuecomment-578519701
    private async Task<AsyncDuplexStreamingCall<Poker.GetInfoRequest, Poker.TableInfo>> StartServerStream() {
        try {
            while (await stream.ResponseStream.MoveNext()) {
                Debug.Log("got info");
                // Exit if application is stopped
                tokenSource.Token.ThrowIfCancellationRequested();

                var info = stream.ResponseStream.Current;
                tableInfo.Set(info);

                Debug.Log(info.ToString());
                ui.UpdateUI(tableInfo, playerID);
            }
        } catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled) {
            Debug.Log("Stream cancelled");
        } catch (OperationCanceledException) {
            stream.Dispose();
        } catch (RpcException ex) {
            Debug.Log(ex.ToString());
        } catch (Exception ex) {
            Debug.Log($"Server reading thread failed: {ex.ToString()}");
            Application.Quit();
        }

        Debug.Log("Exiting server info thread...");
        tokenSource.Cancel();
        return null;
    }


    // Update is called once per frame
    private void Update() {

    }

    private void OnApplicationQuit() {
        tokenSource.Cancel();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }
}
