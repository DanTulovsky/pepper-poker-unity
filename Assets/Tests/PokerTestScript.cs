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
            };

            var winners = new RepeatedField<Winners>
            {
                new Winners { Ids = { "player1ID"} },  // level0
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

            var response = game.WinningPlayers();
            Assert.AreEqual(1, response.Count());
        }
    }
}
