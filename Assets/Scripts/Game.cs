using System;
using Poker;
using Google.Protobuf.Collections;
using System.Collections.Generic;
using System.Linq;

public class Game
{
    // the current instance of GameData proto
    private GameData current = new GameData {Info = new GameInfo()};
    private readonly object locker = new object();

    public long PlayerRealPosition
    {
        get
        {
            lock (locker)
            {
                return playerPosition;
            }
        }
        set
        {
            lock (locker)
            {
                playerPosition = value;
            }
        }
    }

    private int maxPlayers = 7;
    private long playerPosition;

    // Set a new instance of Game
    public void Set(GameData newGameData)
    {
        lock (locker)
        {
            current = newGameData;
        }
    }

    // GetCopy returns a copy of tableInfo (to be used in UIs)
    public GameData Copy
    {
        get
        {
            lock (locker)
            {
                return current?.Clone();
            }
        }
    }

    public long? SmallBlind
    {
        get
        {
            lock (locker)
            {
                return current?.Info?.SmallBlind;
            }
        }
    }

    public long BigBlind
    {
        get
        {
            lock (locker)
            {
                return current.Info.BigBlind;
            }
        }
    }

    public static bool HasState(IEnumerable<PlayerState> states, PlayerState s)
    {
        return states.Any(state => state == s);
    }

    public GameState? GameState
    {
        get
        {
            lock (locker)
            {
                return current?.Info?.GameState;
            }
        }
    }

    // GameFinished returns true when the game is over
    public bool GameFinished()
    {
        return current?.Info?.GameState >= Poker.GameState.Finished;
    }

    // WaitTurnNum returns the WaitTurnNum
    public long WaitTurnNum()
    {
        lock (locker)
        {
            return current?.WaitTurnNum ?? -1;
        }
    }

    // TablePosition returns the shifted player position
    // The human always shows up at position 3
    public int TablePosition(Player p)
    {
        const int wantHumanPosition = 3;

        int realHumanPosition = Convert.ToInt32(PlayerRealPosition);

        int r = Convert.ToInt32(p.Position + (wantHumanPosition - realHumanPosition)) % (maxPlayers);
        if (r < 0)
            return maxPlayers + r;
        else
            return r;
    }

    /// <summary>
    /// Returns the player at the table position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>Player</returns>
    public Player PlayerAtTablePosition(int pos)
    {
        lock (locker)
        {
            foreach (Player player in Players())
            {
                if (TablePosition(player) == pos) return player;
            }
        }

        return null;
    }

    
    public bool IsButton(Player p)
    {
        lock (locker)
        {
            return p.Position == current.Info.ButtonPosition;
        }
    }

    public bool IsSmallBlind(Player p)
    {
        lock (locker)
        {
            return p.Position == current.Info.SmallBlindPosition;
        }
    }

    public bool IsBigBlind(Player p)
    {
        lock (locker)
        {
            return p.Position == current.Info.BigBlindPosition;
        }
    }

    // WaitTurnTimeMaxSec returns WaitTurnTimeMaxSec
    public long WaitTurnTimeMaxSec()
    {
        lock (locker)
        {
            return current.WaitTurnTimeMaxSec;
        }
    }

    // WaitTurnTimeLeftSec returns WaitTurnTimeLeftSec
    public long? WaitTurnTimeLeftSec()
    {
        lock (locker)
        {
            return current?.WaitTurnTimeLeftSec;
        }
    }

    // CommunityCards returns the community cards
    public Poker.CommunityCards CommunityCards()
    {
        lock (locker)
        {
            return current?.Info?.CommunityCards;
        }
    }

    public int NumCommunityCards()
    {
        lock (locker)
        {
            return current?.Info?.CommunityCards?.Card.Count ?? 0;
        }
    }

    // WaitTurnID returns the WaitTurnId
    public string WaitTurnID()
    {
        lock (locker)
        {
            return current?.WaitTurnID;
        }
    }

    public TimeSpan GameStartsIn()
    {
        int t;
        lock (locker)
        {
            t = Convert.ToInt32(current?.Info?.GameStartsInSec);
        }

        return TimeSpan.FromSeconds(t);
    }

    public TimeSpan GameStartsInMax()
    {
        int t;
        lock (locker)
        {
            t = Convert.ToInt32(current?.Info?.GameStartsInMaxSec);
        }

        return TimeSpan.FromSeconds(t);
    }

    // IsMyTurn returns true if it's my turn
    public bool IsMyTurn(string playerID, long lastTurnID)
    {
        lock (locker)
        {
            return current?.WaitTurnID == playerID && lastTurnID < current?.WaitTurnNum;
        }
    }

    public Player PlayerFromID(string id)
    {
        lock (locker)
        {
            return current.Info.Players.FirstOrDefault(p => p.Id == id);
        }
    }

    public Player MyInfo()
    {
        lock (locker)
        {
            return current.Player;
        }
    }

    public RepeatedField<Player> Players()
    {
        lock (locker)
        {
            return current?.Info.Players;
        }
    }


    public List<List<Player>> WinningPlayers()
    {
        var levels = new List<List<Player>>();

        lock (locker)
        {
            levels.AddRange(current.Info.WinningIds.Select(l1 => l1.Ids.Select(PlayerFromID).ToList()));
        }

        return levels;
    }

    public long PlayerStack(Player player)
    {
        lock (locker)
        {
            return player?.Money?.Stack ?? 0;
        }
    }

    public long PlayerTotalBetThisHand(Player player)
    {
        lock (locker)
        {
            return player?.Money?.BetThisHand ?? 0;
        }
    }

    public long PlayerCurrentBet(Player player)
    {
        lock (locker)
        {
            return player?.Money?.BetThisHand ?? 0;
        }
    }

    public long Pot(Player player)
    {
        lock (locker)
        {
            return player?.Money?.Pot ?? 0;
        }
    }
}

public class PlayerNotFoundException : Exception
{
    public PlayerNotFoundException()
    {
    }

    public PlayerNotFoundException(string message)
        : base(message)
    {
    }

    public PlayerNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }
}