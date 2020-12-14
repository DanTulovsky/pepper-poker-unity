using System;
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

    [Header("Input Fields")] public TMP_InputField playerUsernameInput;
    public TMP_InputField playerPasswordInput;
    public TMP_InputField betAmountInput;


    [Header("Table Game Objects")] public QUI_Window gameStartsInfo;
    public GameObject gameStartsRadialBar;
    public GameObject communityCardLocation;
    public QUI_Window winnersWindow;
    public TMP_Text winnersWindowHeading;
    public GameObject buttonToken;
    public GameObject smallBlindToken;
    public GameObject bigBlindToken;
    
    public Object cardBlankPrefab;
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

        ShowCommunityCards(game.CommunityCards());

        if (game.GameFinished())
        {
            ShowWinners();
        }
        else
        {
            winnersWindow.SetActive(false);
            winnersWindowHeading.SetText("");
        }

        lastGameState = game.GameState.GetValueOrDefault();
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
                    StartCoroutine(Manager.Instance.tablePositions[pos].PlayWinnerParticles(delay));
                    winnersWindowHeading.SetText(player.Combo);
                }
            }
        }
    }


    private void ShowCommunityCards(CommunityCards cc)
    {
        const int offset = 180; // cards next to each other
        GameObject parent = communityCardLocation;

        RemoveChildren(parent);
        for (int i = 0;
            i < cc?.Card.Count;
            i++)
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



    public static void RemoveChildren(GameObject parent)
    {
        for (int i = 0;
            i < parent.transform.childCount;
            i++)
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


// Start is called before the first frame update
    public void Start()
    {
        game = Manager.Instance.Game;
        Assert.IsNotNull(game);
        
        clientInfo = Manager.Instance.ClientInfo;
        Assert.IsNotNull(clientInfo);

        Assert.IsNotNull(Manager.Instance.tablePositions);
        
        radialBarGameStart = gameStartsRadialBar.GetComponent<QUI_Bar>();
        buttonToken.SetActive(false);
        smallBlindToken.SetActive(false);
        bigBlindToken.SetActive(false);
        
        foreach (TablePosition t in Manager.Instance.tablePositions)
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