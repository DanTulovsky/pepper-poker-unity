using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Poker;
using static Poker.CardRank;
using static Poker.CardSuit;

namespace Tests
{
    public class PokerTestScript
    {
        private static Game TestGameData()
        {
            // Create players
            var players = new RepeatedField<Player>
            {
                new Player
                {
                  Name = "player1",
                  Id = "player1ID",
                  Position = 0,
                  Money = new PlayerMoney
                  {
                      Bank = 9000,
                      Stack = 1000,
                      Winnings = 100,
                  },
                  Card = { new RepeatedField<Card> 
                      {
                          new Card { Suite = Club, Rank = Eight},
                          new Card { Suite = Club, Rank = Seven},
                      } 
                  },
                  Combo = "TwoPair",
                },
                
                new Player
                {
                  Name = "player2",
                  Id = "player2ID",
                  Position = 1,
                  Money = new PlayerMoney
                  {
                      Bank = 4000,
                      Stack = 1100,
                      Winnings = 10,
                  },
                  Card = { new RepeatedField<Card> 
                      {
                          new Card { Suite = Heart, Rank = Eight},
                          new Card { Suite = Spade, Rank = Six},
                      } 
                  },
                  Combo = "TwoPair",
                },
            };

            var winners = new RepeatedField<Winners>
            {
                new Winners { Ids = { "player1ID"} },  // level0
                new Winners { Ids = { "player2ID"} },  // level2
            };
        
            GameData info = new GameData
            {
                Info = new GameInfo
                {
                    TableName = "testTable",
                    TableID = "testTableID",
                    GameState = GameState.WaitingPlayers,
                    GameStartsInSec = 0,
                    AckToken = "",
                    
                    CommunityCards = new CommunityCards(),
                    
                    MaxPlayers = 5,
                    MinPlayers = 2,
                    BigBlind = 10,
                    SmallBlind = 5,
                    Buyin = 1000,
                    
                    Players = { players },
                    WinningIds = { winners },
                }
            };

            Game gd = new Game();
            gd.Set(info);

           return gd;
        }
        
        [Test]
        public void WinningPlayers1Passes()
        {
            Game game = TestGameData();

            var response = game.WinningPlayers().ToList();
            Assert.AreEqual(2, response.Count);
            Assert.AreEqual("player1ID", response[0][0].Id);
            Assert.AreEqual("player2ID", response[1][0].Id);
        }
    }
}
