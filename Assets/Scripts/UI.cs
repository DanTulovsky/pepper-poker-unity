using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf.Collections;
using Humanizer;
using Poker;
using QuantumTek.QuantumUI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using Resources = UnityEngine.Resources;

public class UI : MonoBehaviour {

    [Header("Text Fields")]
    public TMP_Text playerUsernameDisplay;
    public TMP_Text blindsDisplay;
    public TMP_Text tableStatusDisplay;
    public TMP_Text gameStartsTime;

    [Header("Money Text Fields")]
    public TMP_Text stackAmount;
    public TMP_Text bankAmount;
    public TMP_Text totalBetThisHandAmount;
    public TMP_Text minBetThisRoundAmount;
    public TMP_Text currentBetAmount;
    public TMP_Text potAmount;
    public TMP_Text nextPlayerName;

    [Header("Input Fields")]
    public TMP_InputField playerUsernameInput;
    public TMP_InputField playerPasswordInput;
    public TMP_InputField betAmountInput;


   [Header("Table Game Objects")]
    public QUI_Window gameStartsInfo;
    public GameObject gameStartsRadialBar;
    public GameObject communityCardLocation;
    public QUI_Window winnersWindow;
    public TMP_Text winnersList;
    public List<GameObject> tablePositions;

    private Object cardBlankPrefab;
    private Game game;
    private ClientInfo clientInfo;

    private QUI_Bar radialBarGameStart;
    
    private readonly Dictionary<int,TMP_Text> positionNames = new Dictionary<int, TMP_Text>();
    private readonly Dictionary<int,TMP_Text> lastActions = new Dictionary<int, TMP_Text>();
    private readonly Dictionary<int,TMP_Text> stacks = new Dictionary<int, TMP_Text>();
    private readonly Dictionary<int, QUI_Bar> radialBars = new Dictionary<int, QUI_Bar>();
    private readonly Dictionary<int, GameObject> positionCards = new Dictionary<int, GameObject>();


    // Update updates the UI based on game
    private void UpdateUI() {
        if (game == null) {
            return;
        }
        
        string blinds = $"${game.SmallBlind()} / ${game.BigBlind()}";
        blindsDisplay.SetText(blinds);

        // Table and round status
        tableStatusDisplay.SetText(game.GameState().ToString());

        // Time to game start
        TimeSpan startsIn = game.GameStartsIn();
        if (startsIn.Seconds > 0) {
            gameStartsInfo.SetActive(true);
            gameStartsTime.SetText(startsIn.Humanize());

            float fillAmount = 1 - (float)startsIn.Seconds / 100;
            radialBarGameStart.SetFill(fillAmount);
        } else {
            gameStartsInfo.SetActive(false);
        }

        Player player = game.MyInfo();
        if (player is null) { return; }

        // Stack
        string stack = $"${player.Money?.Stack.ToString()}";
        stackAmount.SetText(stack);

        // Bank
        string bank = $"${player.Money?.Bank.ToString().Humanize()}";
        bankAmount.SetText(bank.Humanize());
        
        // Total bet this hand
        string totalBetThisHand = $"${player.Money?.BetThisHand.ToString().Humanize()}";
        totalBetThisHandAmount.SetText(totalBetThisHand);

        // Current bet
        string currentBet = $"${player.Money?.BetThisRound.ToString().Humanize()}";
        currentBetAmount.SetText(currentBet);

        // Minimum bet this round
        string minBetThisRound = $"${player.Money?.MinBetThisRound.ToString().Humanize()}";
        minBetThisRoundAmount.SetText(minBetThisRound);
        
        // Pot
        string pot = $"${player.Money?.Pot.ToString().Humanize()}";
        potAmount.SetText(pot);

        // Next player
        Player nextPlayer = game.PlayerFromID(game.WaitTurnID());
        string nextName = nextPlayer?.Name;
        string nextID = nextPlayer?.Id;
        nextPlayerName.SetText(nextName);

        ShowCommunityCards(game.CommunityCards());

        // Per player settings
        if (game.Players() == null) return;
        
        int pos = Convert.ToInt32(game.MyInfo().Position);
        positionNames[pos].SetText($"{game.MyInfo().Name}");
            
        foreach (Player p in game.Players())
        {
            pos = Convert.ToInt32(p.Position);

            TimeSpan turnTimeLeft;
            turnTimeLeft = TimeSpan.FromSeconds(p.Id == nextID ? Convert.ToInt32(game.WaitTurnTimeLeftSec()) : 0);

            // Name
            positionNames[pos].SetText($"{p.Name}");

            // LastAction
            string lastAction = p.LastAction.Action == PlayerAction.None ? "" : p.LastAction.Action.ToString();
            lastActions[pos]
                .SetText(p.LastAction.Amount > 0 ? $"{lastAction} (${p.LastAction.Amount})" : $"{lastAction}");

            // Stacks
            stacks[pos].SetText($"${p.Money.Stack}");
            
            // positionChips[pos].OutlineWidth = 150;
            // positionChips[pos].OutlineColor = Color.cyan;
            // positionChips[pos].enabled = p.Id == nextID;
            
            // Turn timeout bars
            radialBars[pos].gameObject.SetActive(false);
            
            // Player whose turn it is
            if (p.Id == nextID)
            {
                radialBars[pos].gameObject.SetActive(true);
                float fillAmount = turnTimeLeft.Seconds / (float)game.WaitTurnTimeMaxSec();
                radialBars[pos].SetFill(fillAmount);
            }

            if (p.Id == clientInfo.PlayerID)
            {
                CardsAtPosition(game.MyInfo().Card, Convert.ToInt32(game.MyInfo().Position));
            }
            else
            {
                FaceDownCardsAtPosition(pos);
            }
        }
        ShowWinners();
    }

    // ShowWinners displays the winning window
    private void ShowWinners()
    {
        if (!game.GameFinished())
        {
            winnersWindow.SetActive(false);
            return;
        }
        
        winnersWindow.SetActive(true);
        
        var winners = game.Winners();
        winnersList.SetText(string.Join("\n", winners));

        var winningPlayers = game.WinningPlayers();
        Debug.Log(winningPlayers.Humanize());
    }
    
    private void ShowCommunityCards(CommunityCards cc) {

        int offset = 180; // cards next to each other
        GameObject parent = communityCardLocation;
        RemoveChildren(parent);

        for (int i = 0; i < cc?.Card.Count; i++) {
            string file = Cards.FileForCard(cc.Card[i]);
            Object cardPrefab = Resources.Load(file);
            if (cardPrefab == null) {
                throw new FileNotFoundException(file + " not file found - please check the configuration");
            }

            GameObject cardObject = Instantiate(cardPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
            Assert.IsNotNull(cardObject);

            // Debug.Assert(cardObject != null, nameof(cardObject) + " != null");
            
            cardObject.transform.parent = parent.transform;
            cardObject.transform.Rotate(new Vector3(-90, 0, 0));
            Vector3 position = parent.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        }
    }

    // CardsAtPosition puts face up cards at the given position
    private void CardsAtPosition(RepeatedField<Card> hole, int pos) {

        const int offset = 180;
        GameObject parent = positionCards[pos];
        RemoveChildren(parent);

        for (int i = 0; i < hole.Count; i++) {
            string file = Cards.FileForCard(hole[i]);
            Object cardPrefab = Resources.Load(file);
            if (cardPrefab == null) {
                throw new FileNotFoundException(file + " not file found - please check the configuration");
            }

            GameObject cardObject = Instantiate(cardPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
            Assert.IsNotNull(cardObject);
            // Debug.Assert(cardObject != null, nameof(cardObject) + " != null");
            
            cardObject.transform.parent = parent.transform;
            cardObject.transform.Rotate(new Vector3(-90, 0, 0));
            Vector3 position = parent.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        }
    }

    // FaceDownCardAtPosition places 2 face down cards the given position
    private void FaceDownCardsAtPosition(int pos) {
        const int offset = 100; // cards overlapping

        GameObject parent = positionCards[pos];
        RemoveChildren(parent);

        for (int i = 0; i < 2; i++) {
            GameObject cardObject = Instantiate(cardBlankPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
            Debug.Assert(cardObject != null, nameof(cardObject) + " != null");
            
            cardObject.transform.parent = parent.transform;
            cardObject.transform.Rotate(new Vector3(-90, 180, 0));
            Vector3 position = parent.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        }
    }

    private static void RemoveChildren(GameObject parent) {
        for (int i = parent.transform.childCount - 1; i >= 0; i--) {
            GameObject child = parent.transform.GetChild(i).gameObject;
            child.SetActive(false); // hide right away
        }

        for (int i = parent.transform.childCount - 1; i >= 0; i--) {
            GameObject child = parent.transform.GetChild(i).gameObject;
            DestroyImmediate(child);
        }
    }

    // Start is called before the first frame update
    private void Start() {
        game = Manager.Instance.Game;
        Assert.IsNotNull(game);
        clientInfo = Manager.Instance.ClientInfo;
        Assert.IsNotNull(clientInfo);

        radialBarGameStart = gameStartsRadialBar.GetComponent<QUI_Bar>();

       for (int i = 0; i < tablePositions.Count; i++)
        {
            positionNames[i] = tablePositions[i].transform.Find("Name").gameObject.GetComponent<TMP_Text>();
            positionNames[i].SetText("");
            
            lastActions[i] = tablePositions[i].transform.Find("LastAction").gameObject.GetComponent<TMP_Text>();
            lastActions[i].SetText("");
            
            stacks[i] = tablePositions[i].transform.Find("Stack").gameObject.GetComponent<TMP_Text>();
            stacks[i].SetText("");
            
            // positionChips[i] = tablePositions[i].transform.Find("Chip").gameObject.GetComponent<Outline>();
            radialBars[i] = tablePositions[i].transform.Find("Radial Bar").gameObject.GetComponent<QUI_Bar>();
            radialBars[i].gameObject.SetActive(false);
            
            positionCards[i] = tablePositions[i].transform.Find("Cards").gameObject;
        }
            
        cardBlankPrefab = Resources.Load(Cards.BlankCard());
        if (cardBlankPrefab == null) {
            throw new FileNotFoundException(Cards.BlankCard() + " no file found - please check the configuration");
        }

        // winnersWindow.SetActive(false);
        // gameStartsInfo.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateUI();
    }
}

public static class ExtensionMethod {
    public static void SetActiveRecursivelyExt(this GameObject obj, bool state) {
        foreach (Transform child in obj.transform) {
            child.gameObject.SetActiveRecursivelyExt(state);
        }
        obj.SetActive(state);
    }
}