using System;
using Poker;
using Google.Protobuf.Collections;
using System.Collections.Generic;
using System.Linq;

public class Game
{
    // the current instance of GameData proto
    private GameData _current = new GameData {Info = new GameInfo()};
    private readonly object _locker = new object();

    public long PlayerRealPosition
    {
        get
        {
            lock (_locker)
            {
                return _playerPosition;
            }
        }
        set
        {
            lock (_locker)
            {
                _playerPosition = value;
            }
        }
    }

    private int _maxPlayers = 7;
    private long _playerPosition;

    // Set a new instance of Game
    public void Set(GameData newGameData)
    {
        lock (_locker)
        {
            _current = newGameData;
        }
    }

    // GetCopy returns a copy of tableInfo (to be used in UIs)
    public GameData Copy
    {
        get
        {
            lock (_locker)
            {
                return _current?.Clone();
            }
        }
    }

    public long? SmallBlind
    {
        get
        {
            lock (_locker)
            {
                return _current?.Info?.SmallBlind;
            }
        }
    }

    public long BigBlind
    {
        get
        {
            lock (_locker)
            {
                return _current.Info.BigBlind;
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
            lock (_locker)
            {
                return _current?.Info?.GameState;
            }
        }
    }

    // GameFinished returns true when the game is over
    public bool GameFinished()
    {
        return _current?.Info?.GameState >= Poker.GameState.Finished;
    }

    // WaitTurnNum returns the WaitTurnNum
    public long WaitTurnNum()
    {
        lock (_locker)
        {
            return _current?.WaitTurnNum ?? -1;
        }
    }

    // TablePosition returns the shifted player position
    // The human always shows up at position 3
    public int TablePosition(Player p)
    {
        const int wantHumanPosition = 3;

        int realHumanPosition = Convert.ToInt32(PlayerRealPosition);

        int r = Convert.ToInt32(p.Position + (wantHumanPosition - realHumanPosition)) % (_maxPlayers);
        if (r < 0)
            return _maxPlayers + r;
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
        lock (_locker)
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
        lock (_locker)
        {
            return p.Position == _current.Info.ButtonPosition;
        }
    }

    public bool IsSmallBlind(Player p)
    {
        lock (_locker)
        {
            return p.Position == _current.Info.SmallBlindPosition;
        }
    }

    public bool IsBigBlind(Player p)
    {
        lock (_locker)
        {
            return p.Position == _current.Info.BigBlindPosition;
        }
    }

    // WaitTurnTimeMaxSec returns WaitTurnTimeMaxSec
    public long WaitTurnTimeMaxSec()
    {
        lock (_locker)
        {
            return _current.WaitTurnTimeMaxSec;
        }
    }

    // WaitTurnTimeLeftSec returns WaitTurnTimeLeftSec
    public long? WaitTurnTimeLeftSec()
    {
        lock (_locker)
        {
            return _current?.WaitTurnTimeLeftSec;
        }
    }

    // CommunityCards returns the community cards
    public Poker.CommunityCards CommunityCards()
    {
        lock (_locker)
        {
            return _current?.Info?.CommunityCards;
        }
    }

    public int NumCommunityCards()
    {
        lock (_locker)
        {
            return _current?.Info?.CommunityCards?.Card.Count ?? 0;
        }
    }

    // WaitTurnID returns the WaitTurnId
    public string WaitTurnID()
    {
        lock (_locker)
        {
            return _current?.WaitTurnID;
        }
    }

    public TimeSpan GameStartsIn()
    {
        int t;
        lock (_locker)
        {
            t = Convert.ToInt32(_current?.Info?.GameStartsInSec);
        }

        return TimeSpan.FromSeconds(t);
    }

    public TimeSpan GameStartsInMax()
    {
        int t;
        lock (_locker)
        {
            t = Convert.ToInt32(_current?.Info?.GameStartsInMaxSec);
        }

        return TimeSpan.FromSeconds(t);
    }

    // IsMyTurn returns true if it's my turn
    public bool IsMyTurn(string playerID, long lastTurnID)
    {
        lock (_locker)
        {
            return _current?.WaitTurnID == playerID && lastTurnID < _current?.WaitTurnNum;
        }
    }

    public Player PlayerFromID(string id)
    {
        lock (_locker)
        {
            return _current.Info.Players.FirstOrDefault(p => p.Id == id);
        }
    }

    public Player MyInfo()
    {
        lock (_locker)
        {
            return _current.Player;
        }
    }

    public RepeatedField<Player> Players()
    {
        lock (_locker)
        {
            return _current?.Info.Players;
        }
    }


    public List<List<Player>> WinningPlayers()
    {
        var levels = new List<List<Player>>();

        lock (_locker)
        {
            levels.AddRange(_current.Info.WinningIds.Select(l1 => l1.Ids.Select(PlayerFromID).ToList()));
        }

        return levels;
    }

    public long PlayerStack(Player player)
    {
        lock (_locker)
        {
            return player?.Money?.Stack ?? 0;
        }
    }

    public long PlayerTotalBetThisHand(Player player)
    {
        lock (_locker)
        {
            return player?.Money?.BetThisHand ?? 0;
        }
    }

    public long PlayerCurrentBet(Player player)
    {
        lock (_locker)
        {
            return player?.Money?.BetThisHand ?? 0;
        }
    }

    public long Pot(Player player)
    {
        lock (_locker)
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