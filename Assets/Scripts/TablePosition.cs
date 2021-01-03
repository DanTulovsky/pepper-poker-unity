using System;
using System.Collections;
using System.Collections.Generic;
using Humanizer;
using Poker;
using QuantumTek.QuantumUI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

// ReSharper disable HeapView.BoxingAllocation

public class TablePosition : MonoBehaviour
{
    public GameObject playerNamePosition;
    public GameObject lastActionPosition;
    public TMP_Text stackText;
    public QUI_Bar radialBar;
    public GameObject cardsPosition;
    public Transform tokenPosition;

    public ParticleSystem pSystem;
    public TMP_Text nameText;

    public int index;

    private Player _myPlayer;
    private PlayerAction _previousAction;
    [SerializeField] public Avatar avatar;

    public void Awake()
    {
        playerNamePosition = gameObject.transform.Find("Name").gameObject;
        lastActionPosition = gameObject.transform.Find("LastAction").gameObject;
        stackText = gameObject.transform.Find("Stack").gameObject.GetComponent<TMP_Text>();
        radialBar = gameObject.transform.Find("Radial Bar").gameObject.GetComponent<QUI_Bar>();
        cardsPosition = gameObject.transform.Find("Cards").gameObject;
        tokenPosition = gameObject.transform.Find("Token");
        
        nameText = playerNamePosition.GetComponent<TMP_Text>();
        pSystem = playerNamePosition.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    private void Update()
    {
        _myPlayer = Manager.Instance.game.PlayerAtTablePosition(index);
        Game game = Manager.Instance.game;

        if (_myPlayer == null)
        {
            // reset UI
            nameText.SetText($"");
            // TODO: LastAction
            stackText.SetText($"");
            radialBar.gameObject.SetActive(false);

            GameObject parent = cardsPosition;
            UI.RemoveChildren(parent);

            return;
        }

        // Next player
        Player nextPlayer = game.PlayerFromID(game.WaitTurnID());
        string nextID = nextPlayer?.Id;

        // Name
        nameText.SetText($"{_myPlayer.Name}");
        nameText.alpha = 1.0f;
        if (Game.HasState(_myPlayer.State, PlayerState.Folded))
        {
            nameText.alpha = 0.5f;
        }

        // LastAction
        // string lastAction = myPlayer.LastAction.Action == PlayerAction.None
        //     ? ""
        //     : myPlayer.LastAction.Action.ToString();
        // lastActionText
        //     .SetText(myPlayer.LastAction.Amount > 0
        //         ? $"{lastAction} (${myPlayer.LastAction.Amount})"
        //         : $"{lastAction}");

        if (_myPlayer.LastAction.Action != PlayerAction.None)
        {
            // Debug.Log($"> {index} previous: {previousAction}; now: {myPlayer.LastAction}");
            if (_previousAction != _myPlayer.LastAction.Action)
            {
                var currentState = Manager.Instance.game.GameState;
                StartCoroutine(ShowLastAction(TimeSpan.FromSeconds(5), currentState));
            }
        }

        // Stacks
        stackText.SetText($"${_myPlayer.Money.Stack}");

        // Turn timeout bars
        radialBar.gameObject.SetActive(false);

        // tokens
        ShowToken(_myPlayer);

        // Player whose turn it is
        if (_myPlayer.Id == nextID)
        {
            TimeSpan turnTimeLeft;
            turnTimeLeft =
                TimeSpan.FromSeconds(_myPlayer.Id == nextID ? Convert.ToInt32(game.WaitTurnTimeLeftSec()) : 0);

            radialBar.gameObject.SetActive(true);
            float fillAmount = turnTimeLeft.Seconds / (float) game.WaitTurnTimeMaxSec();
            radialBar.SetFill(fillAmount);
        }

        ShowCards();

        // Set additional info for the local player
        if (_myPlayer.Id == game.MyInfo().Id)
        {
            UpdateLocalPlayer();
        }
    }

    private IEnumerator ShowLastAction(TimeSpan delay, GameState? currentState)
    {
        _previousAction = _myPlayer.LastAction.Action;

        DateTime start = DateTime.Now;

        GameObject prefab;
        switch (_myPlayer.LastAction.Action)
        {
            case PlayerAction.Call:
                prefab = Manager.Instance.uiUpdater.actionCallPrefab;
                break;
            case PlayerAction.Check:
                prefab = Manager.Instance.uiUpdater.actionCheckPrefab;
                break;
            case PlayerAction.Bet:
                prefab = Manager.Instance.uiUpdater.actionBetPrefab;
                break;
            case PlayerAction.Fold:
                prefab = Manager.Instance.uiUpdater.actionFoldPrefab;
                break;
            case PlayerAction.AllIn:
                prefab = Manager.Instance.uiUpdater.actionAllInPrefab;
                break;
            default:
                yield break;
        }

        Assert.IsNotNull(prefab);

        GameObject actionPrefab =
            Instantiate(prefab, new Vector3(0, 0, -1), Quaternion.identity);
        actionPrefab.transform.SetParent(lastActionPosition.transform);
        actionPrefab.transform.localPosition = Vector3.zero;
        Vector3 originalScale = actionPrefab.transform.localScale;
        actionPrefab.LeanScale(originalScale*1.6f, TimeSpan.FromSeconds(1.0f).Seconds);

        // TODO: Add bet amount in popup
        // string lastAction = myPlayer.LastAction.Action == PlayerAction.None
        //     ? ""
        //     : myPlayer.LastAction.Action.ToString();
        // lastActionText
        //     .SetText(myPlayer.LastAction.Amount > 0
        //         ? $"{lastAction} (${myPlayer.LastAction.Amount})"
        //         : $"{lastAction}");


        // wait until state changes
        yield return new WaitUntil(() => Manager.Instance.game.GameState != currentState);

        // but at least "delay seconds"
        yield return new WaitUntil(() => DateTime.Now.Subtract(start) > delay);

        GameObject.Destroy(actionPrefab);
        _previousAction = PlayerAction.None;
    }

    private void ShowCards()
    {
        Game game = Manager.Instance.game;

        if (Game.HasState(_myPlayer.State, PlayerState.Folded))
        {
            UI.RemoveChildren(cardsPosition);
            return;
        }

        if (Manager.Instance.game.GameFinished())
        {
            CardsAtPosition(_myPlayer.Card);
        }
        else
        {
            if (_myPlayer.Id == game.MyInfo().Id)
            {
                CardsAtPosition(game.MyInfo().Card);
            }
            else
            {
                if (Manager.Instance.game.GameState > GameState.ReadyToStart)
                {
                    FaceDownCardsAtPosition();
                }
            }
        }
    }

// FaceDownCardAtPosition places 2 face down cards the given position
    private void FaceDownCardsAtPosition()
    {
        UI ui = Manager.Instance.uiUpdater;

        const int offset = 100; // cards overlapping

        UI.RemoveChildren(cardsPosition);
        for (int i = 0; i < 2; i++)
        {
            GameObject cardObject =
                Instantiate(ui.cardBlankPrefab, new Vector3(0, 0, -1), Quaternion.identity);

            Assert.IsNotNull(cardObject);

            cardObject.transform.parent = cardsPosition.transform;
            cardObject.transform.Rotate(new Vector3(-90, 180, 0));
            Vector3 position = cardsPosition.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z - i);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        }
    }

// CardsAtPosition puts face up cards at the given position
    private void CardsAtPosition(IReadOnlyList<Card> hole)
    {
        const int offset = 180;

        UI.RemoveChildren(cardsPosition);

        for (int i = 0; i < hole.Count; i++)
        {
            string file = Cards.FileForCard(hole[i]);
            UnityEngine.Object cardPrefab = Resources.Load(file);
            GameObject cardObject = Instantiate(cardPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
            Assert.IsNotNull(cardObject);

            cardObject.transform.parent = cardsPosition.transform;
            cardObject.transform.Rotate(new Vector3(-90, 0, 0));
            Vector3 position = cardsPosition.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        }
    }

    /// <summary>
    /// Updates local player UI data
    /// </summary>
    private void UpdateLocalPlayer()
    {
        UI ui = Manager.Instance.uiUpdater;

        if (_myPlayer?.Money == null) return;

        // Stack
        string stack = $"${_myPlayer.Money?.Stack.ToString()}";
        ui.stackAmount.SetText(stack);

        stackText.SetText($"${_myPlayer.Money.Stack}");

        // Bank
        string bank = $"${_myPlayer.Money?.Bank.ToString().Humanize()}";
        ui.bankAmount.SetText(bank.Humanize());

        // Total bet this hand
        string totalBetThisHand = $"${_myPlayer.Money?.BetThisHand.ToString().Humanize()}";
        ui.totalBetThisHandAmount.SetText(totalBetThisHand);

        // Current bet
        string currentBet = $"${_myPlayer.Money?.BetThisRound.ToString().Humanize()}";
        ui.currentBetAmount.SetText(currentBet);

        // Minimum bet this round
        string minBetThisRound = $"${_myPlayer.Money?.MinBetThisRound.ToString().Humanize()}";
        ui.minBetThisRoundAmount.SetText(minBetThisRound);

        // Pot
        string pot = $"${_myPlayer.Money?.Pot.ToString().Humanize()}";
        ui.potAmount.SetText(pot);
    }

    /// <summary>
    /// Shows the token next to the player.
    /// </summary>
    /// <param name="p"></param>
    private void ShowToken(Player p)
    {
        Game game = Manager.Instance.game;

        if (game.IsButton(p))
        {
            ShowTokenButton();
        }

        if (game.IsSmallBlind(p))
        {
            ShowTokenSmallBlind();
        }

        if (game.IsBigBlind(p))
        {
            ShowTokenBigBlind();
        }
    }

    /// <summary>
    /// Shows the button token at the parent.
    /// </summary>
    private void ShowTokenButton()
    {
        UI ui = Manager.Instance.uiUpdater;

        ui.buttonToken.transform.SetParent(tokenPosition, false);
        ui.buttonToken.transform.localPosition = new Vector3(0, 0, -2);
        ui.buttonToken.SetActive(true);
    }

    /// <summary>
    /// Shows the smallBlind token at the parent.
    /// </summary>
    private void ShowTokenSmallBlind()
    {
        UI ui = Manager.Instance.uiUpdater;

        ui.smallBlindToken.transform.SetParent(tokenPosition, false);
        ui.smallBlindToken.transform.localPosition = new Vector3(0, 0, -2);
        ui.smallBlindToken.SetActive(true);
    }

    /// <summary>
    /// Shows the bigBlind token at the parent.
    /// </summary>
    private void ShowTokenBigBlind()
    {
        UI ui = Manager.Instance.uiUpdater;

        ui.bigBlindToken.transform.SetParent(tokenPosition, false);
        ui.bigBlindToken.transform.localPosition = new Vector3(0, 0, -2);
        ui.bigBlindToken.SetActive(true);
    }

    public IEnumerator PlayWinnerParticles(TimeSpan delay)
    {
        // raise winning cards
        if (_myPlayer != null)
        {
            var winCards = _myPlayer.Hand;
        }
        
        yield return new WaitForSeconds(delay.Seconds);
        // iterate over community cards and player hole and
        // raise the ones that are in winCards
        
        
        
        pSystem.Play(true);
    }

    public void StopWinnerParticles()
    {
        pSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}