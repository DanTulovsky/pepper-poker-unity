﻿using System;
using System.Collections;
using System.Collections.Generic;
using Humanizer;
using Poker;
using QuantumTek.QuantumUI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class TablePosition : MonoBehaviour
{
    public GameObject playerNamePosition;
    public TMP_Text lastActionText;
    public TMP_Text stackText;
    public QUI_Bar radialBar;
    public GameObject cardsPosition;
    public Transform tokenPosition;

    public ParticleSystem pSystem;
    public TMP_Text nameText;

    public int index;

    private Player myPlayer;

    public void Awake()
    {
        playerNamePosition = gameObject.transform.Find("Name").gameObject;
        lastActionText = gameObject.transform.Find("LastAction").gameObject.GetComponent<TMP_Text>();
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
        myPlayer = Manager.Instance.Game.PlayerAtTablePosition(index);
        Game game = Manager.Instance.Game;

        if (myPlayer == null)
        {
            // reset UI
            nameText.SetText($"");
            lastActionText.SetText($"");
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
        nameText.SetText($"{myPlayer.Name}");
            nameText.alpha = 1.0f;
        if (Game.HasState(myPlayer.State, PlayerState.Folded))
        {
            nameText.alpha = 0.5f;
        }

        // LastAction
        string lastAction = myPlayer.LastAction.Action == PlayerAction.None
            ? ""
            : myPlayer.LastAction.Action.ToString();
        lastActionText
            .SetText(myPlayer.LastAction.Amount > 0
                ? $"{lastAction} (${myPlayer.LastAction.Amount})"
                : $"{lastAction}");

        // Stacks
        stackText.SetText($"${myPlayer.Money.Stack}");

        // Turn timeout bars
        radialBar.gameObject.SetActive(false);

        // tokens
        ShowToken(myPlayer, this);

        // Player whose turn it is
        if (myPlayer.Id == nextID)
        {
            TimeSpan turnTimeLeft;
            turnTimeLeft =
                TimeSpan.FromSeconds(myPlayer.Id == nextID ? Convert.ToInt32(game.WaitTurnTimeLeftSec()) : 0);

            radialBar.gameObject.SetActive(true);
            float fillAmount = turnTimeLeft.Seconds / (float) game.WaitTurnTimeMaxSec();
            radialBar.SetFill(fillAmount);
        }

        ShowCards();

        // Set additional info for the local player
        if (myPlayer.Id == game.MyInfo().Id)
        {
            UpdateLocalPlayer();
        }
    }

    private void ShowCards()
    {
        Game game = Manager.Instance.Game;
        
        if (Game.HasState(myPlayer.State, PlayerState.Folded))
        {
            UI.RemoveChildren(cardsPosition);
            return;
        }
        
        if (Manager.Instance.Game.GameFinished()) 
        {
            CardsAtPosition(myPlayer.Card);
        }
        else
        {
            if (myPlayer.Id == game.MyInfo().Id)
            {
                CardsAtPosition(game.MyInfo().Card);
            }
            else
            {
                if (Manager.Instance.Game.GameState > GameState.ReadyToStart)
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
                Instantiate(ui.cardBlankPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;

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
    /// Updates local plaer UI data
    /// </summary>
    private void UpdateLocalPlayer()
    {
        UI ui = Manager.Instance.uiUpdater;
            
        if (myPlayer?.Money == null) return;

        // Stack
        string stack = $"${myPlayer.Money?.Stack.ToString()}";
        ui.stackAmount.SetText(stack);

        stackText.SetText($"${myPlayer.Money.Stack}");

        // Bank
        string bank = $"${myPlayer.Money?.Bank.ToString().Humanize()}";
        ui.bankAmount.SetText(bank.Humanize());

        // Total bet this hand
        string totalBetThisHand = $"${myPlayer.Money?.BetThisHand.ToString().Humanize()}";
        ui.totalBetThisHandAmount.SetText(totalBetThisHand);

        // Current bet
        string currentBet = $"${myPlayer.Money?.BetThisRound.ToString().Humanize()}";
        ui.currentBetAmount.SetText(currentBet);

        // Minimum bet this round
        string minBetThisRound = $"${myPlayer.Money?.MinBetThisRound.ToString().Humanize()}";
        ui.minBetThisRoundAmount.SetText(minBetThisRound);

        // Pot
        string pot = $"${myPlayer.Money?.Pot.ToString().Humanize()}";
        ui.potAmount.SetText(pot);
    }

    /// <summary>
    /// Shows the token next to the player.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="position"></param>
    private void ShowToken(Player p, TablePosition position)
    {
        Game game = Manager.Instance.Game;
        
        if (game.IsButton(p))
        {
            ShowTokenButton(position.tokenPosition);
        }

        if (game.IsSmallBlind(p))
        {
            ShowTokenSmallBlind(position.tokenPosition);
        }

        if (game.IsBigBlind(p))
        {
            ShowTokenBigBlind(position.tokenPosition);
        }
    }

    /// <summary>
    /// Shows the button token at the parent.
    /// </summary>
    /// <param name="parent"></param>
    private static void ShowTokenButton(Transform parent)
    {
        UI ui = Manager.Instance.uiUpdater;
        
        ui.buttonToken.transform.SetParent(parent, false);
        ui.buttonToken.transform.localPosition = new Vector3(0, 0, -2);
        ui.buttonToken.SetActive(true);
    }

    /// <summary>
    /// Shows the smallBlind token at the parent.
    /// </summary>
    /// <param name="parent"></param>
    private static void ShowTokenSmallBlind(Transform parent)
    {
        UI ui = Manager.Instance.uiUpdater;
        
        ui.smallBlindToken.transform.SetParent(parent, false);
        ui.smallBlindToken.transform.localPosition = new Vector3(0, 0, -2);
        ui.smallBlindToken.SetActive(true);
    }

    /// <summary>
    /// Shows the bigBlind token at the parent.
    /// </summary>
    /// <param name="parent"></param>
    private static void ShowTokenBigBlind(Transform parent)
    {
        UI ui = Manager.Instance.uiUpdater;
        
        ui.bigBlindToken.transform.SetParent(parent, false);
        ui.bigBlindToken.transform.localPosition = new Vector3(0, 0, -2);
        ui.bigBlindToken.SetActive(true);
    }

    public IEnumerator PlayWinnerParticles(TimeSpan delay)
    {
        yield return new WaitForSeconds(delay.Seconds);
        pSystem.Play(true);
    }

    public void StopWinnerParticles()
    {
        pSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}