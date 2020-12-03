using Google.Protobuf.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using Humanizer;
using Poker;
using QuantumTek.QuantumUI;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class UI : MonoBehaviour {

    [Header("Text Fields")]
    public TMP_Text playerUsernameDisplay;
    public TMP_Text blindsDisplay;
    public TMP_Text tableStatusDisplay;
    public TMP_Text gameStartsTime;

    [Header("Money Text Fields")]
    public TMP_Text stackAmount;
    public TMP_Text totalBetThisHandAmount;
    public TMP_Text currentBetAmount;
    public TMP_Text potAmount;
    public TMP_Text nextPlayerName;
    public TMP_Text betAmount;

    [Header("Input Fields")]
    public TMP_InputField playerUsernameInput;
    public TMP_InputField playerPasswordInput;
    public TMP_InputField minBetAmount;


    [FormerlySerializedAs("GameStartsInfo")] [Header("Table Game Objects")]
    public GameObject gameStartsInfo;
     public GameObject gameStartsRadialBar;
     public GameObject communityCardLocation;
    public List<GameObject> tablePositions;

    private UnityEngine.Object cardBlankPrefab;


    // Update updates the UI based on gameData
    public void UpdateUI(GameData gameData, string playerID) {

        Poker.GameData current = gameData.GetCopy();
        string blinds = $"${current?.Info.SmallBlind} / ${current?.Info.BigBlind}";
        blindsDisplay.SetText(blinds);

        // Table and round status
        tableStatusDisplay.SetText(current?.Info.GameState.ToString());

        // Time to game start
        TimeSpan startsIn = gameData.GameStartsIn();
        if (startsIn.Seconds > 0) {
            gameStartsInfo.SetActiveRecursivelyExt(true);
            gameStartsTime.SetText(startsIn.Humanize());

            QUI_Bar bar = gameStartsRadialBar.GetComponent<QUI_Bar>();
            float fillAmount = 1 - (float)startsIn.Seconds / 100;
            bar.SetFill(fillAmount);
        } else {
            gameStartsInfo.SetActiveRecursivelyExt(false);
        }

        Player player = MyInfo(current, playerID);
        if (player is null) { return; }

        // Stack
        string stack = $"${player?.Money?.Stack.ToString()}";
        stackAmount.SetText(stack);

        // Total bet this hand
        string totalBetThisHand = $"${player?.Money?.BetThisHand.ToString()}";
        totalBetThisHandAmount.SetText(totalBetThisHand);

        // Current bet
        string currentBet = $"${player?.Money?.BetThisRound.ToString()}";
        currentBetAmount.SetText(currentBet);

        // Pot
        string pot = $"${player?.Money?.Pot.ToString()}";
        potAmount.SetText(pot);

        // Next player
        Player nextPlayer = gameData.PlayerFromID(current?.WaitTurnID);
        string nextName = nextPlayer?.Name;
        string nextID = nextPlayer?.Id;
        nextPlayerName.SetText(nextName);

        ShowCommunityCards(current?.Info.CommunityCards);

        // int turnTimeLeftSec = Convert.ToInt32(current?.Info.turnTimeLeft);
        // TimeSpan turnTimeLeft = TimeSpan.FromSeconds(turnTimeLeftSec);

        // Per player settings
        foreach (Player pi in current?.Info.Players) {
            int pos = Convert.ToInt32(pi.Position);

            // Name
            GameObject nameObject = tablePositions[pos].transform.Find("Name").gameObject;
            // nameObject.GetComponent<TMP_Text>().SetText($"{pi.Name} ({turnTimeLeft.Humanize()})");

            GameObject chipObject = tablePositions[pos].transform.Find("Chip").gameObject;
            Outline chipOutline = chipObject.GetComponent<Outline>();
            chipOutline.OutlineWidth = 150;
            chipOutline.OutlineColor = Color.cyan;

            chipOutline.enabled = pi.Id == nextID;

            if (pi.Id == playerID) {
                CardsAtPosition(pi.Card, pos);
                continue;
            }
            FaceDownCardsAtPosition(pos);
        }
    }

    // myInfo returns the info for the current player
    private Player MyInfo(Poker.GameData gameData, string playerID)
    {
        return gameData.Player;
   }

    private void ShowCommunityCards(Poker.CommunityCards cc) {

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
            cardObject.transform.parent = parent.transform;
            cardObject.transform.Rotate(new Vector3(-90, 0, 0));
            Vector3 position = parent.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        }
    }

    // CardsAtPosition puts face up cards at the given position
    private void CardsAtPosition(RepeatedField<Poker.Card> hole, int pos) {

        int offset = 180;
        GameObject parent = tablePositions[pos].transform.Find("Cards").gameObject;
        RemoveChildren(parent);

        for (int i = 0; i < hole.Count; i++) {
            string file = Cards.FileForCard(hole[i]);
            Object cardPrefab = Resources.Load(file);
            if (cardPrefab == null) {
                throw new FileNotFoundException(file + " not file found - please check the configuration");
            }

            GameObject cardObject = Instantiate(cardPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
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
        int offset = 100; // cards overlapping

        GameObject parent = tablePositions[pos].transform.Find("Cards").gameObject;
        RemoveChildren(parent);

        for (int i = 0; i < 2; i++) {
            GameObject cardObject = Instantiate(cardBlankPrefab, new Vector3(0, 0, -1), Quaternion.identity) as GameObject;
            cardObject.transform.parent = parent.transform;
            cardObject.transform.Rotate(new Vector3(-90, 180, 0));
            Vector3 position = parent.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        };
    }

    private void RemoveChildren(GameObject parent) {
        for (int i = parent.transform.childCount - 1; i >= 0; i--) {
            GameObject child = parent.transform.GetChild(i).gameObject;
            child.SetActive(false); // hide right away
        }

        for (int i = parent.transform.childCount - 1; i >= 0; i--) {
            GameObject child = parent.transform.GetChild(i).gameObject;
            Destroy(child);
        }
    }

    // Start is called before the first frame update
    void Start() {
        cardBlankPrefab = Resources.Load(Cards.BlankCard());
        if (cardBlankPrefab == null) {
            throw new FileNotFoundException(Cards.BlankCard() + " not file found - please check the configuration");
        }

        gameStartsInfo.SetActiveRecursivelyExt(false);
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