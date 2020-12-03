using System;
using Poker;

public class GameData {
    // the current instance of GameData
    private Poker.GameData current = new Poker.GameData();
    private readonly object locker = new object();


    // Set a new instance of GameData
    public void Set(Poker.GameData newGameData) {
        lock (locker) {
            current = newGameData;
        }
    }

    // GetCopy returns a copy of tableInfo (to be used in UIs)
    public Poker.GameData GetCopy() {
        lock (locker) {
            return current?.Clone();
        }
    }


    // GameFinished returns true when the game is over
    public bool GameFinished()
    {
        return current.Info.GameState == GameState.PlayingDone;
    }

    // WaitTurnNum returns the WaitTurnNum
    public long WaitTurnNum() {
        lock (locker) {
            return current?.WaitTurnNum ?? -1;
        }
    }


    public TimeSpan GameStartsIn() {
        int t;
        lock (locker) {
            t = Convert.ToInt32(current?.Info.GameStartsInSec);
        }
        return TimeSpan.FromSeconds(t);
    }

    // IsMyTurn returns true if it's my turn
    public bool IsMyTurn(string playerID, long lastTurnID) {
        lock (locker) {
            return current?.WaitTurnID == playerID && lastTurnID < current?.WaitTurnNum;
        }

    }

    // myInfo returns a copy of the info for the current player
    public Player PlayerFromID() {
        lock (locker) {
            return current.Player;
        }
    }

    public long PlayerStack(Player player) {
        lock (locker) {
            return player?.Money?.Stack ?? 0;
        }
    }

    public long PlayerTotalBetThisHand(Player player) {
        lock (locker) {
            return player?.Money?.BetThisHand ?? 0;
        }
    }

    public long PlayerCurrentBet(Player player) {
        lock (locker) {
            return player?.Money?.BetThisHand ?? 0;
        }
    }

    public long Pot(Player player) {
        lock (locker) {
            return player?.Money?.Pot ?? 0;
        }
    }
}

public class PlayerNotFoundException : Exception {
    public PlayerNotFoundException() {
    }

    public PlayerNotFoundException(string message)
        : base(message) {
    }

    public PlayerNotFoundException(string message, Exception inner)
        : base(message, inner) {
    }
}