using System;
using UnityEngine;
using Grpc.Core;
using TMPro;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Poker;


public class Manager : MonoBehaviour {
    public TMP_Text playerNameDisplay;
    public TMP_Text playerStackDisplay;
    public TMP_Text blindsDisplay;
    public TMP_Text tableStatusDisplay;
    public TMP_Text roundStatusDisplay;

    private PokerClient pokerClient;
    private string playerID;
    private string tableID = "";
    private string roundID = "";

    private Poker.TableInfo tableInfo = new Poker.TableInfo { };
    private readonly object tableInfoLocker = new object();

    // Post-round start cancellation token
    private System.Threading.CancellationTokenSource tokenSource = new System.Threading.CancellationTokenSource { };

    // bi-direction streaming
    private AsyncDuplexStreamingCall<Poker.GetInfoRequest, Poker.TableInfo> stream;

    private Grpc.Core.Logging.LogLevelFilterLogger logger;

    public TMPro.TMP_Text playerNameInput;

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
        playerNameDisplay.SetText(playerNameInput.text);
        playerID = pokerClient.SayHello(playerNameDisplay.text);

        Debug.Log("PlayerID: " + playerID.ToString());
        Debug.Log("Player Name: " + playerNameDisplay.text);

        // Now join table
        tokenSource = new System.Threading.CancellationTokenSource();

        StartCoroutine(nameof(JoinTable));
        Debug.Log("leaving SayHello");
    }

    private IEnumerator JoinTable() {
        Debug.Log("Joining table...");
        tableID = pokerClient.JoinTable(tableID, playerID);

        Debug.Log("Table ID: " + this.tableID.ToString());

        // Kick off background refresh thread for tableInfo
        StartInfoStream();

        // Wait for round to start
        Debug.Log("Waiting for round to start...");
        yield return new WaitUntil(() => tableInfo.RoundID != "");
        roundID = tableInfo.RoundID;
        Debug.Log("Round ID: " + tableInfo.RoundID);
    }

    // fileForCard returns the filename for the given card
    // This is specific to the PlayingCards plugin we are using.
    private string FileForCard(Poker.Card card) {
        var prefix = "Prefab/BackColor_Red/Red_PlayingCards_";
        var fmt = "00.##";

        var rank = (int)card.Rank + 1;
        var suit = card.Suite.ToString();

        var file = $"{prefix}{suit}{rank.ToString(fmt)}_00";

        return file;
    }

    public void TestCommunityCards() {
        var cc = new Poker.CommunityCards();

        var card = new Poker.Card {
            Rank = Poker.CardRank.Ace,
            Suite = Poker.CardSuit.Club,
        };
        cc.Card.Add(card);

        card = new Poker.Card {
            Rank = Poker.CardRank.Queen,
            Suite = Poker.CardSuit.Heart,
        };
        cc.Card.Add(card);
        card = new Poker.Card {
            Rank = Poker.CardRank.Two,
            Suite = Poker.CardSuit.Diamond,
        };
        cc.Card.Add(card);
        card = new Poker.Card {
            Rank = Poker.CardRank.Jack,
            Suite = Poker.CardSuit.Club,
        };
        cc.Card.Add(card);
        card = new Poker.Card {
            Rank = Poker.CardRank.Eight,
            Suite = Poker.CardSuit.Club,
        };
        cc.Card.Add(card);

        ShowCommunityCards(cc);
    }

    public void ShowCommunityCards(Poker.CommunityCards cards) {

        var offset = 180;

        for (var i = 0; i < cards.Card.Count; i++) {
            var file = FileForCard(cards.Card[i]);
            var cardPrefab = Resources.Load(file);
            if (cardPrefab == null) {
                throw new FileNotFoundException(file + " not file found - please check the configuration");
            }

            var parent = GameObject.Find("Card0");
            var cardObject = Instantiate(cardPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
            cardObject.transform.parent = parent.transform;
            cardObject.transform.Rotate(new Vector3(-90, 0, 0));
            var position = parent.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        }
    }

    public void DealFlop() {
        var cards = new List<string> { };
        cards.Add("Prefab/BackColor_Red/Red_PlayingCards_Club01_00");
        cards.Add("Prefab/BackColor_Red/Red_PlayingCards_Club06_00");
        cards.Add("Prefab/BackColor_Red/Red_PlayingCards_Club11_00");

        for (var i = 0; i < cards.Count; i++) {
            var file = cards[i];
            var cardPrefab = Resources.Load(file);
            if (cardPrefab == null) {
                throw new FileNotFoundException(file + " not file found - please check the configuration");
            }

            var parent = GameObject.Find("Flop" + i.ToString());
            var cardObject = Instantiate(cardPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
            cardObject.transform.parent = parent.transform;
            cardObject.transform.Rotate(new Vector3(-90, 0, 0));
            cardObject.transform.position = parent.transform.position;
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        }
    }

    public void DealTurn() {
        var card = "Prefab/BackColor_Red/Red_PlayingCards_Diamond03_00";

        var file = card;
        var cardPrefab = Resources.Load(file);
        if (cardPrefab == null) {
            throw new FileNotFoundException(file + " not file found - please check the configuration");
        }

        var parent = GameObject.Find("Turn");
        var cardObject = Instantiate(cardPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
        cardObject.transform.parent = parent.transform;
        cardObject.transform.Rotate(new Vector3(-90, 0, 0));
        cardObject.transform.position = parent.transform.position;
        cardObject.transform.localScale = new Vector3(12, 12, 12);
    }

    public void DealRiver() {
        var card = "Prefab/BackColor_Red/Red_PlayingCards_Heart09_00";

        var file = card;
        var cardPrefab = Resources.Load(file);
        if (cardPrefab == null) {
            throw new FileNotFoundException(file + " not file found - please check the configuration");
        }

        var parent = GameObject.Find("River");
        var cardObject = Instantiate(cardPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
        cardObject.transform.parent = parent.transform;
        cardObject.transform.Rotate(new Vector3(-90, 0, 0));
        cardObject.transform.position = parent.transform.position;
        cardObject.transform.localScale = new Vector3(12, 12, 12);
    }

    public void ActionAllIn() {
        // TODO: Fix amount
        var amount = 0;
        pokerClient.ActionBet(tableID, playerID, tableInfo.RoundID, amount);
    }

    public void ActionCheck() {
        pokerClient.ActionCheck(tableID, playerID, tableInfo.RoundID);
    }

    public void ActionCall() {
        // TODO: Fix amount
        var amount = 0;
        pokerClient.ActionBet(tableID, playerID, tableInfo.RoundID, amount);
    }

    public void ActionFold() {
        pokerClient.ActionFold(tableID, playerID, tableInfo.RoundID);
    }

    public void ActionRaise() {
        // TODO: Fix amount
        var amount = 0;
        pokerClient.ActionBet(tableID, playerID, tableInfo.RoundID, amount);
    }

    private void StartInfoStream() {
        var call = pokerClient.GetInfoStreaming(playerID, tableID, "");
        this.stream = call;

        Debug.Log("Starting server stream listener...");
        StartCoroutine(nameof(StartServerStream));

        // Send client request
        StartCoroutine(nameof(StartClientStream));
    }

    // SendClientRequest sends a client request to the server
    private IEnumerator StartClientStream()
    {
        while (true) {
            var req = new Poker.GetInfoRequest
            {
                PlayerID = playerID,
                TableID = tableID,
                RoundID = roundID,
            };
            Debug.Log("Sending client request...");
            Debug.Log("playerID: " + playerID);
            Debug.Log("tableID: " + tableID);
            Debug.Log("roundID: " + roundID);

            // Exit if application is stopped
            tokenSource.Token.ThrowIfCancellationRequested();

            stream.RequestStream.WriteAsync(req);

            yield return new WaitForSeconds(2);
        }
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
                Poker.TableInfo infoCopy;

                lock (tableInfoLocker) {
                    tableInfo = info;
                    Debug.Log(info.ToString());
                    infoCopy = tableInfo.Clone();
                }
                UpdateUI(infoCopy);
                Debug.Log(infoCopy.ToString());
            }
        } catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled) {
            Debug.Log("Stream cancelled");
        } catch (OperationCanceledException) {
            stream.Dispose();
        } catch (RpcException ex) {
            Debug.Log(ex.ToString());
        }

        return null;
    }

    private Poker.Player MyInfo(Poker.TableInfo ti) {
        Debug.Log("Getting my info...");
        foreach (var p in ti.Player) {
            Debug.Log("Checking player...");
            if (p.Id != playerID) continue;
            Debug.Log("Found player..");
            return p;
        }

        Debug.Log("Failed to find player...");
        return null;
    }

    // UpdateUI updates the UI with info from tableInfo
    // tableInfo should be a copy!
    private void UpdateUI(Poker.TableInfo tableInfoCopy) {
        Debug.Log("Updating UI...");
        var player = MyInfo(tableInfoCopy);

        var stack = $"${player?.Money?.Stack.ToString()}";
        playerStackDisplay.SetText(stack);

        var blinds = $"${tableInfoCopy?.SmallBlind} / ${tableInfoCopy?.BigBlind}";
        blindsDisplay.SetText(blinds);

        tableStatusDisplay.SetText(tableInfoCopy?.TableStatus.ToString());
        roundStatusDisplay.SetText(tableInfoCopy?.RoundStatus.ToString());
    }

    // Update is called once per frame
    private void Update() {

    }

    private void OnApplicationQuit() {
        tokenSource.Cancel();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }
}
