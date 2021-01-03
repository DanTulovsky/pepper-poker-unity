using System.IO;
using Poker;
using UnityEngine;
using UnityEngine.Assertions;

public class CommunityCards : MonoBehaviour
{
    private int _shownCommunityCards;
    private Game _game;

    public GameObject communityCardLocation;

    // Start is called before the first frame update
    private void Start()
    {
        _game = Manager.Instance.game;
        Assert.IsNotNull(_game);
    }

    // Update is called once per frame
    private void Update()
    {
        ShowCommunityCards();

        if (Manager.Instance.game.GameState == GameState.WaitingPlayers)
        {
            _shownCommunityCards = 0;
            UI.RemoveChildren(communityCardLocation);
        }
    }

    private void ShowCommunityCards()
    {
        Poker.CommunityCards cc = Manager.Instance.game.CommunityCards();

        if (_shownCommunityCards >= Manager.Instance.game.NumCommunityCards())
        {
            return;
        }

        Debug.Log($"showing cc... {Manager.Instance.game.GameState}");
        const int offset = 180; // cards next to each other

        UI.RemoveChildren(communityCardLocation);
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

            cardObject.transform.parent = communityCardLocation.transform;
            cardObject.transform.Rotate(new Vector3(-90, 0, 0));
            Vector3 position = communityCardLocation.transform.position;
            cardObject.transform.position = new Vector3(
                position.x + i * offset, position.y, position.z);
            cardObject.transform.localScale = new Vector3(12, 12, 12);
        }

        _shownCommunityCards = Manager.Instance.game.NumCommunityCards();
    }
}