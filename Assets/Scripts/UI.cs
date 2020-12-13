using System;
using System.Collections.Generic;
using System.IO;
using Humanizer;
using Poker;
using QuantumTek.QuantumUI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using Resources = UnityEngine.Resources;

public class UI : MonoBehaviour
{
    [Header("Text Fields")] public TMP_Text playerUsernameDisplay;
    public TMP_Text blindsDisplay;
    public TMP_Text tableStatusDisplay;
    public TMP_Text gameStartsTime;

    [Header("Money Text Fields")] public TMP_Text stackAmount;
    public TMP_Text bankAmount;
    public TMP_Text totalBetThisHandAmount;
    public TMP_Text minBetThisRoundAmount;
    public TMP_Text currentBetAmount;
    public TMP_Text potAmount;
    public TMP_Text nextPlayerName;

    [Header("Input Fields")] public TMP_InputField playerUsernameInput;
    public TMP_InputField playerPasswordInput;
    public TMP_InputField betAmountInput;


    [Header("Table Game Objects")] public QUI_Window gameStartsInfo;
    public GameObject gameStartsRadialBar;
    public GameObject communityCardLocation;
    public QUI_Window winnersWindow;
    public TMP_Text winnersWindowHeading;
    public List<TablePosition> tablePositions;
    public GameObject buttonToken;
    public GameObject smallBlindToken;
    public GameObject bigBlindToken;

    private Object cardBlankPrefab;
    private Game game;
    private GameState lastGameState;
    private ClientInfo clientInfo;

    private QUI_Bar radialBarGameStart;


    // Update updates the UI based on game
    private void UpdateUI()
    {
        if (game == null)
        {
            return;
        }

        string blinds = $"${game.SmallBlind} / ${game.BigBlind}";
        blindsDisplay.SetText(blinds);

        // Table and round status
        tableStatusDisplay.SetText(game.GameState.ToString());

        // Time to game start
        TimeSpan startsIn = game.GameStartsIn();
        TimeSpan startsInMax = game.GameStartsInMax();

        if (startsIn.Seconds > 0)
        {
            gameStartsInfo.SetActive(true);
            gameStartsTime.SetText($"{startsIn.Humanize()}");

            float fillAmount = 1 - startsIn.Seconds / (float) startsInMax.Seconds;
            radialBarGameStart.SetFill(fillAmount);
        }
        else
        {
            gameStartsInfo.SetActive(false);
        }

        ShowLocalPlayer();
        ShowCommunityCards(game.CommunityCards());
        ShowPlayers();

        if (game.GameFinished())
        {
            ShowWinners();
        }
        else
        {
            winnersWindow.SetActive(false);
            winnersWindowHeading.SetText("");
            ShowCards();
        }

        lastGameState = game.GameState.GetValueOrDefault();
    }

    private void ShowLocalPlayer()
    {
        Player localPlayer = game.MyInfo();
        if (localPlayer is null)
        {
            return;
        }

        // local player name
        int pos = game.TablePosition(game.MyInfo());
        tablePositions[pos].nameText.SetText($"{game.MyInfo().Name}");
       
        // Stack
        string stack = $"${localPlayer.Money?.Stack.ToString()}";
        stackAmount.SetText(stack);

        // Bank
        string bank = $"${localPlayer.Money?.Bank.ToString().Humanize()}";
        bankAmount.SetText(bank.Humanize());

        // Total bet this hand
        string totalBetThisHand = $"${localPlayer.Money?.BetThisHand.ToString().Humanize()}";
        totalBetThisHandAmount.SetText(totalBetThisHand);

        // Current bet
        string currentBet = $"${localPlayer.Money?.BetThisRound.ToString().Humanize()}";
        currentBetAmount.SetText(currentBet);
       
        // Minimum bet this round
        string minBetThisRound = $"${localPlayer.Money?.MinBetThisRound.ToString().Humanize()}";
        minBetThisRoundAmount.SetText(minBetThisRound);

        // Pot
        string pot = $"${localPlayer.Money?.Pot.ToString().Humanize()}";
        potAmount.SetText(pot);
    }
    
    private void ShowPlayers()
    {
        // Per player settings
        if (game.Players() == null) return;

        // Next player
        Player nextPlayer = game.PlayerFromID(game.WaitTurnID());
        string nextName = nextPlayer?.Name;
        string nextID = nextPlayer?.Id;
        nextPlayerName.SetText(nextName);
        
        foreach (Player p in game.Players())
        {
            TablePosition position = tablePositions[game.TablePosition(p)];

            TimeSpan turnTimeLeft;
            turnTimeLeft = TimeSpan.FromSeconds(p.Id == nextID ? Convert.ToInt32(game.WaitTurnTimeLeftSec()) : 0);

            // Name
            position.nameText.SetText($"{p.Name}");
            if (Game.HasState(p.State, PlayerState.Folded))
            {
                position.nameText.alpha = (float) 0.5;
            }

            // LastAction
            string lastAction = p.LastAction.Action == PlayerAction.None ? "" : p.LastAction.Action.ToString();
            position.lastActionText
                .SetText(p.LastAction.Amount > 0 ? $"{lastAction} (${p.LastAction.Amount})" : $"{lastAction}");

            // Stacks
            position.stackText.SetText($"${p.Money.Stack}");

            // Turn timeout bars
            position.radialBar.gameObject.SetActive(false);

            // tokens
            ShowToken(p, position);
            
            // Player whose turn it is
            if (p.Id == nextID)
            {
                position.radialBar.gameObject.SetActive(true);
                float fillAmount = turnTimeLeft.Seconds / (float) game.WaitTurnTimeMaxSec();
                position.radialBar.SetFill(fillAmount);
            }
        }
    }
    
    private void ShowToken(Player p, TablePosition position)
    {
        if (game.IsButton(p)) { ShowTokenButton(position.tokenPosition); }
        if (game.IsSmallBlind(p)) { ShowTokenSmallBlind(position.tokenPosition); }
        if (game.IsBigBlind(p)) { ShowTokenBigBlind(position.tokenPosition); }
    }
    
    private void ShowCards()
    {
        foreach (Player p in game.Players())
        {
            int pos = game.TablePosition(p);

            if (Game.HasState(p.State, PlayerState.Folded))
            {
                GameObject parent = tablePositions[pos].cards;
                RemoveChildren(parent);
                continue;
            }

            if (p.Id == clientInfo.PlayerID)
            {
                CardsAtPosition(game.MyInfo().Card, game.TablePosition(game.MyInfo()));
            }
            else
            {
                FaceDownCardsAtPosition(pos);
            }
        }
    }

    // ShowWinners displays the winning window
    private void ShowWinners()
    {
        // Only run this once per finished game
        if (lastGameState == GameState.Finished) return;

        winnersWindow.SetActive(true);

        var winningPlayers = game.WinningPlayers();

        for (int j = 0; j < winningPlayers.Count; j++)
        {
            var level = winningPlayers[j];

            for (int i = 0; i < level.Count; i++)
            {
                Player player = level[i];

                int pos = game.TablePosition(player);
                TimeSpan delay = TimeSpan.FromSeconds(i * j * 5);

                if (player.Money.Winnings > 0)
                {
                    StartCoroutine(tablePositions[pos].PlayWinnerParticles(delay));

                    winnersWindowHeading.SetText(player.Combo);
                }

                CardsAtPosition(player.Card, pos);
            }
        }
    }


    private void ShowCommunityCards(CommunityCards cc)
    {
        int offset = 180; // cards next to each other
        GameObject parent = communityCardLocation;
        RemoveChildren(parent);

        for (int i = 0; i < cc?.Card.Count; i++)
        {
            string file = Cards.FileForCard(cc.Card[i]);
            Object cardPrefab = Resources.Load(file);
            if (cardPrefab == null)
            {
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
    private void CardsAtPosition(IReadOnlyList<Card> hole, int pos)
    {
        const int offset = 180;
        GameObject parent = tablePositions[pos].cards;
        RemoveChildren(parent);

        for (int i = 0; i < hole.Count; i++)
        {
            string file = Cards.FileForCard(hole[i]);
            Object cardPrefab = Resources.Load(file);
            if (cardPrefab == null)
            {
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
    private void FaceDownCardsAtPosition(int pos)
    {
        const int offset = 100; // cards overlapping

        GameObject parent = tablePositions[pos].cards;
        RemoveChildren(parent);

        for (int i = 0; i < 2; i++)
        {
            GameObject cardObject =
                Instantiate(cardBlankPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
            Debug.Assert(cardObject != null, nameof(cardObject) + " != null");

            cardObject.transform.parent = parent.transform;
            cardObject.transform.Rotate(new Vector3(-90, 180, 0));
            Vector3 position = parent.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z-i);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        }
    }

    private static void RemoveChildren(GameObject parent)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            GameObject child = parent.transform.GetChild(i).gameObject;
            child.SetActive(false); // hide right away
        }

        for (int i = parent.transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.transform.GetChild(i).gameObject;
            DestroyImmediate(child);
        }
    }

    /// <summary>
    /// Shows the button token at the parent.
    /// </summary>
    /// <param name="parent"></param>
    private void ShowTokenButton(Transform parent)
    {
        buttonToken.transform.SetParent(parent, false);
        buttonToken.transform.localPosition = new Vector3(0,0, -2);
        buttonToken.SetActive(true);
    }

    /// <summary>
    /// Shows the smallBlind token at the parent.
    /// </summary>
    /// <param name="parent"></param>
    private void ShowTokenSmallBlind(Transform parent)
    {
        smallBlindToken.transform.SetParent(parent, false);
        smallBlindToken.transform.localPosition = new Vector3(0, 0, -2);
        smallBlindToken.SetActive(true);
    }

    /// <summary>
    /// Shows the bigBlind token at the parent.
    /// </summary>
    /// <param name="parent"></param>
    private void ShowTokenBigBlind(Transform parent)
    {
        bigBlindToken.transform.SetParent(parent, false);
        bigBlindToken.transform.localPosition = new Vector3(0, 0, -2);
        bigBlindToken.SetActive(true);
    }
    // Start is called before the first frame update
    public void Start()
    {
        game = Manager.Instance.Game;
        Assert.IsNotNull(game);

        clientInfo = Manager.Instance.ClientInfo;
        Assert.IsNotNull(clientInfo);

        radialBarGameStart = gameStartsRadialBar.GetComponent<QUI_Bar>();

        buttonToken.SetActive(false);
        smallBlindToken.SetActive(false);
        bigBlindToken.SetActive(false);

        foreach (TablePosition t in tablePositions)
        {
            t.nameText.SetText("");
            t.lastActionText.SetText("");
            t.stackText.SetText("");
            t.radialBar.gameObject.SetActive(false);
        }

        cardBlankPrefab = Resources.Load(Cards.BlankCard());
        if (cardBlankPrefab == null)
        {
            throw new FileNotFoundException(Cards.BlankCard() + " no file found - please check the configuration");
        }
    }

    // Update is called once per frame
    public void Update()
    {
        UpdateUI();
    }
}

public static class ExtensionMethod
{
    public static void SetActiveRecursivelyExt(this GameObject obj, bool state)
    {
        foreach (Transform child in obj.transform)
        {
            child.gameObject.SetActiveRecursivelyExt(state);
        }

        obj.SetActive(state);
    }
}