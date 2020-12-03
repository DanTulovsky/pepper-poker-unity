using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests
{
    public class CardsTestScript
    {
        // A Test behaves as an ordinary method
        [Test]
        public void BlankCardPasses()
        {
            Assert.AreEqual(Cards.BlankCard(), "Prefab/BackColor_Red/Red_PlayingCards_Blank_00");
        }

        [Test]
        public void AceCardPasses()
        {
            Poker.Card card = new Poker.Card
            {
                Rank = Poker.CardRank.Ace,
                Suite = Poker.CardSuit.Diamond
            };

            const string expected = "Prefab/BackColor_Red/Red_PlayingCards_Diamond01_00";
            Assert.AreEqual(Cards.FileForCard(card), expected);
        }

        [Test]
        public void TwoCardPasses()
        {
            Poker.Card card = new Poker.Card
            {
                Rank = Poker.CardRank.Two,
                Suite = Poker.CardSuit.Diamond
            };

            const string expected = "Prefab/BackColor_Red/Red_PlayingCards_Diamond02_00";
            Assert.AreEqual(Cards.FileForCard(card), expected);
        }

        [Test]
        public void KingCardPasses()
        {
            Poker.Card card = new Poker.Card
            {
                Rank = Poker.CardRank.King,
                Suite = Poker.CardSuit.Diamond
            };

            const string expected = "Prefab/BackColor_Red/Red_PlayingCards_Diamond13_00";
            Assert.AreEqual(Cards.FileForCard(card), expected);
        }
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator CardsTestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
