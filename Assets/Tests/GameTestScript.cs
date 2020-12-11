using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Poker;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class GameTestScript
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TablePositionPass1()
        {

            foreach (var tc in new List<(int pos, int humanPlayerPos, int want)>
            {
                (pos: 0, humanPlayerPos: 0, want: 3),
                (pos: 1, humanPlayerPos: 0, want: 4),
                
                (pos: 0, humanPlayerPos: 3, want: 0),
                (pos: 3, humanPlayerPos: 3, want: 3),
                
                (pos: 0, humanPlayerPos: 4, want: 6),
                (pos: 2, humanPlayerPos: 4, want: 1),
                (pos: 4, humanPlayerPos: 4, want: 3),
                (pos: 6, humanPlayerPos: 4, want: 5),
                
                (pos: 0, humanPlayerPos: 6, want: 4),
                (pos: 6, humanPlayerPos: 6, want: 3),
            })
            {
                Player p = new Player { Position = tc.pos };  // player being tested
                Game game = new Game {PlayerPosition = tc.humanPlayerPos}; // human player position
            
                Assert.AreEqual(tc.want, game.TablePosition(p));
            }
        }
    }
}
