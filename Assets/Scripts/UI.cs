using Google.Protobuf.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using Humanizer;
using QuantumTek.QuantumUI;

public class UI : MonoBehaviour {

    [Header("Text Fields")]
    public TMP_Text playerNameDisplay;
    public TMP_Text blindsDisplay;
    public TMP_Text tableStatusDisplay;
    public TMP_Text roundStatusDisplay;
    public TMP_Text gameStartsTime;

    [Header("Money Text Fields")]
    public TMP_Text stackAmount;
    public TMP_Text totalBetThisHandAmount;
    public TMP_Text currentBetAmount;
    public TMP_Text potAmount;
    public TMP_Text nextPlayerName;
    public TMP_Text betAmount;

    [Header("Input Fields")]
    public TMP_InputField playerNameInput;
    public TMP_InputField minBetAmount;


    [Header("Table Game Objects")]
    public GameObject GameStartsInfo;
    public GameObject GameStartsRadialBar;
    public GameObject CommunityCardLocation;
    public List<GameObject> tablePositions;

    private UnityEngine.Object cardBlankPrefab;


    // Update updates the UI based on tableInfo
    public void UpdateUI(TableInfo tableInfo, string playerID) {

        Poker.TableInfo current = tableInfo.GetCopy();
        string blinds = $"${current?.SmallBlind} / ${current?.BigBlind}";
        blindsDisplay.SetText(blinds);

        // Table and round status
        tableStatusDisplay.SetText(current?.TableStatus.ToString());
        roundStatusDisplay.SetText(current?.RoundStatus.ToString());

        // Time to game start
        TimeSpan startsIn = tableInfo.GameStartsIn();
        if (startsIn.Seconds > 0) {
            GameStartsInfo.SetActiveRecursivelyExt(true);
            gameStartsTime.SetText(startsIn.Humanize());

            QUI_Bar bar = GameStartsRadialBar.GetComponent<QUI_Bar>();
            float fillAmount = 1 - (float)startsIn.Seconds / 100;
            bar.SetFill(fillAmount);
        } else {
            GameStartsInfo.SetActiveRecursivelyExt(false);
        }

        Poker.Player player = this.MyInfo(current, playerID);
        if (player is null) { return; }

        // Stack
        string stack = $"${player?.Money?.Stack.ToString()}";
        stackAmount.SetText(stack);

        // Total bet this hand
        string totalBetThisHand = $"${player?.Money?.BetThisHand.ToString()}";
        totalBetThisHandAmount.SetText(totalBetThisHand);

        // Currnt bet
        string currentBet = $"${player?.Money?.BetThisRound.ToString()}";
        currentBetAmount.SetText(currentBet);

        // Pot
        string pot = $"${player?.Money?.Pot.ToString()}";
        potAmount.SetText(pot);

        // Next player
        Poker.Player nextPlayer = current.NextPlayer;
        string nextName = nextPlayer?.Name;
        string nextID = nextPlayer?.Id;
        nextPlayerName.SetText(nextName);

        ShowCommunityCards(current.CommunityCards);

        int turnTimeLeftSec = Convert.ToInt32(current.TurnTimeLeftSeconds);
        TimeSpan turnTimeLeft = TimeSpan.FromSeconds(turnTimeLeftSec);

        // Per player settings
        foreach (var pi in current.Player) {
            int pos = Convert.ToInt32(pi.Position);

            // Name
            var nameObject = tablePositions[pos].transform.Find("Name").gameObject;
            nameObject.GetComponent<TMP_Text>().SetText($"{pi.Name} ({turnTimeLeft.Humanize()})");

            var chipObject = tablePositions[pos].transform.Find("Chip").gameObject;
            var chipOutline = chipObject.GetComponent<Outline>();
            chipOutline.OutlineWidth = 150;
            chipOutline.OutlineColor = Color.cyan;

            if (pi.Id == nextID) {
                chipOutline.enabled = true;
            } else {
                chipOutline.enabled = false;
            }

            if (pi.Id == playerID) {
                CardsAtPosition(pi.Card, pos);
                continue;
            }
            FaceDownCardsAtPosition(pos);
        }
    }

    // myInfo returns the info for the current player
    private Poker.Player MyInfo(Poker.TableInfo tableInfo, string playerID) {
        foreach (var p in tableInfo.Player) {
            if (p.Id != playerID) continue;
            return p;
        }
        return null;
    }

    private void ShowCommunityCards(Poker.CommunityCards cc) {

        var offset = 180; // cards next to each other
        var parent = CommunityCardLocation;
        RemoveChildren(parent);

        for (var i = 0; i < cc?.Card.Count; i++) {
            var file = Cards.FileForCard(cc.Card[i]);
            var cardPrefab = Resources.Load(file);
            if (cardPrefab == null) {
                throw new FileNotFoundException(file + " not file found - please check the configuration");
            }

            var cardObject = Instantiate(cardPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
            cardObject.transform.parent = parent.transform;
            cardObject.transform.Rotate(new Vector3(-90, 0, 0));
            var position = parent.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        }
    }

    // CardsAtPosition puts face up cards at the given position
    private void CardsAtPosition(RepeatedField<Poker.Card> hole, int pos) {

        var offset = 180;
        var parent = tablePositions[pos].transform.Find("Cards").gameObject;
        RemoveChildren(parent);

        for (var i = 0; i < hole.Count; i++) {
            var file = Cards.FileForCard(hole[i]);
            var cardPrefab = Resources.Load(file);
            if (cardPrefab == null) {
                throw new FileNotFoundException(file + " not file found - please check the configuration");
            }

            var cardObject = Instantiate(cardPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
            cardObject.transform.parent = parent.transform;
            cardObject.transform.Rotate(new Vector3(-90, 0, 0));
            var position = parent.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        }
    }

    // FaceDownCardAtPosition places 2 face down cards the given position
    private void FaceDownCardsAtPosition(int pos) {
        var offset = 100; // cards overlapping

        var parent = tablePositions[pos].transform.Find("Cards").gameObject;
        RemoveChildren(parent);

        for (int i = 0; i < 2; i++) {
            var cardObject = Instantiate(cardBlankPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
            cardObject.transform.parent = parent.transform;
            cardObject.transform.Rotate(new Vector3(-90, 180, 0));
            var position = parent.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        };
    }

    private void RemoveChildren(GameObject parent) {
        for (int i = parent.transform.childCount - 1; i >= 0; i--) {
            var child = parent.transform.GetChild(i).gameObject;
            child.SetActive(false); // hide right away
        }

        for (int i = parent.transform.childCount - 1; i >= 0; i--) {
            var child = parent.transform.GetChild(i).gameObject;
            Destroy(child);
        }
    }

    // Start is called before the first frame update
    void Start() {
        cardBlankPrefab = Resources.Load(Cards.BlankCard());
        if (cardBlankPrefab == null) {
            throw new FileNotFoundException(Cards.BlankCard() + " not file found - please check the configuration");
        }

        GameStartsInfo.SetActiveRecursivelyExt(false);
    }

    // Update is called once per frame
    void Update() {

    }
}

public static class ExtentionMethod {
    public static void SetActiveRecursivelyExt(this GameObject obj, bool state) {
        obj.SetActive(state);
        foreach (Transform child in obj.transform) {
            SetActiveRecursivelyExt(child.gameObject, state);
        }
    }
}