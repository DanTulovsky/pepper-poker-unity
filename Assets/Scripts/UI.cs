using System;
using System.IO;
using Humanizer;
using Poker;
using QuantumTek.QuantumUI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
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
    public QUI_Window winnersWindow;
    public TMP_Text winnersWindowHeading;

    [Header("Table Game Objects (Tokens)")]
    public GameObject buttonToken;

    public GameObject smallBlindToken;
    public GameObject bigBlindToken;

    [Header("Table Game Objects (Prefabs)")]
    public GameObject actionAllInPrefab;

    public GameObject actionBetPrefab;
    public GameObject actionCallPrefab;
    public GameObject actionCheckPrefab;
    public GameObject actionFoldPrefab;
    public GameObject cardBlankPrefab;

    private Game _game;
    private GameState _lastGameState;
    private ClientInfo _clientInfo;

    private QUI_Bar _radialBarGameStart;

    // Update updates the UI based on game
    private void UpdateUI()
    {
        if (_game == null)
        {
            return;
        }

        string blinds = $"${_game.SmallBlind} / ${_game.BigBlind}";
        blindsDisplay.SetText(blinds);

        // Table and round status
        tableStatusDisplay.SetText(_game.GameState.ToString());

        // Time to game start
        TimeSpan startsIn = _game.GameStartsIn();
        TimeSpan startsInMax = _game.GameStartsInMax();

        if (startsIn.Seconds > 0)
        {
            gameStartsInfo.SetActive(true);
            gameStartsTime.SetText($"{startsIn.Humanize()}");

            float fillAmount = 1 - startsIn.Seconds / (float) startsInMax.Seconds;
            _radialBarGameStart.SetFill(fillAmount);
        }
        else
        {
            gameStartsInfo.SetActive(false);
        }


        if (_game.GameFinished())
        {
            ShowWinners();
        }
        else
        {
            winnersWindow.SetActive(false);
            winnersWindowHeading.SetText("");
        }

        _lastGameState = _game.GameState.GetValueOrDefault();
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
    /// <summary>
    /// Display winners.
    /// </summary>
    private void ShowWinners()
    {
        // Only run this once per finished game
        if (_lastGameState == GameState.Finished) return;
        winnersWindow.SetActive(true);

        var winningPlayers = _game.WinningPlayers();
        for (int j = 0; j < winningPlayers.Count; j++)
        {
            var level = winningPlayers[j];

            for (int i = 0; i < level.Count; i++)
            {
                Player player = level[i];

                int pos = _game.TablePosition(player);
                TimeSpan delay = TimeSpan.FromSeconds(i * j * 2);

                if (player.Money.Winnings > 0)
                {
                    StartCoroutine(Manager.Instance.tablePositions[pos].PlayWinnerParticles(delay));
                    winnersWindowHeading.SetText(player.Combo);
                }
            }
        }
    }


// Start is called before the first frame update
    public void Start()
    {
        _game = Manager.Instance.game;
        Assert.IsNotNull(_game);

        _clientInfo = Manager.Instance.clientInfo;
        Assert.IsNotNull(_clientInfo);

        Assert.IsNotNull(Manager.Instance.tablePositions);

        _radialBarGameStart = gameStartsRadialBar.GetComponent<QUI_Bar>();
        buttonToken.SetActive(false);
        smallBlindToken.SetActive(false);
        bigBlindToken.SetActive(false);

        foreach (TablePosition t in Manager.Instance.tablePositions)
        {
            t.nameText.SetText("");
            t.stackText.SetText("");
            t.radialBar.gameObject.SetActive(false);
        }

        cardBlankPrefab = (GameObject) Resources.Load(Cards.BlankCard());
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