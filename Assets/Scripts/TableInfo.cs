using System;
using UnityEngine;

public class TableInfo {
    // the current instance of TableInfo
    private Poker.TableInfo current = new Poker.TableInfo { };
    private readonly object locker = new object();


    // Set a new instance of TableInfo
    public void Set(Poker.TableInfo newTableInfo) {
        lock (locker) {
            current = newTableInfo;
        }
    }

    // GetCopy returns a copy of tableInfo (to be used in UIs)
    public Poker.TableInfo GetCopy() {
        lock (locker) {
            return current?.Clone();
        }
    }

    // RoundID returns the roundID
    public string RoundID() {
        lock (locker) {
            return current?.RoundID ?? "";
        }
    }

    // GameFinished returns true when the game is over
    public bool GameFinished() {
        return current.TableStatus == Poker.TableStatus.GameFinished;
    }

    // TurnID returns the turnID
    public long TurnID() {
        lock (locker) {
            return current?.TurnID ?? -1;
        }
    }

    // NextTurn returns the Player whose turn it is
    public Poker.Player NextTurn() {
        lock (locker) {
            return current?.NextPlayer;
        }
    }

    // NextTurn returns the Player whose turn it is
    public TimeSpan GameStartsIn() {
        int t = 0;
        lock (locker) {
            t = Convert.ToInt32(current?.GameStartsInSeconds);
        }
        return TimeSpan.FromSeconds(t);
    }

    // IsMyTurn returns true if it's my turn
    public bool IsMyTurn(string playerID, long lastTurnID) {
        lock (locker) {
            return current?.NextPlayer?.Id == playerID && lastTurnID < current?.TurnID;
        }

    }

    // myInfo returns a copy of the info for the current player
    public Poker.Player PlayerFromID(string playerID) {
        lock (locker) {
            foreach (var p in current.Player) {
                if (p.Id != playerID) continue;
                return p;
            }
        }

        throw new PlayerNotFoundException($"Player {0} not found!");
    }

    public long PlayerStack(Poker.Player player) {
        lock (locker) {
            return player?.Money?.Stack ?? 0;
        }
    }

    public long PlayerTotalBetThisHand(Poker.Player player) {
        lock (locker) {
            return player?.Money?.BetThisHand ?? 0;
        }
    }

    public long PlayerCurrentBet(Poker.Player player) {
        lock (locker) {
            return player?.Money?.BetThisHand ?? 0;
        }
    }

    public long Pot(Poker.Player player) {
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