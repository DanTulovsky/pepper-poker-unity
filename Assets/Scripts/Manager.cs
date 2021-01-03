using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Poker;
using QuantumTek.QuantumUI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class RpcErrorEvent : UnityEvent<string>
{
}

public class GameFailedEvent : UnityEvent<string>
{
}

public class Manager : Singleton<Manager>
{
    public UI uiUpdater;
    private PokerClient _pokerClient;
    private long _lastTurnID = -1;
    private string _lastAckToken = "";
    private TablePosition _localHuman;

    [NonSerialized] public readonly ClientInfo clientInfo = new ClientInfo();
    [NonSerialized] public readonly Game game = new Game();
    private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();


    public TMP_InputField serverNameInput;
    public TMP_InputField serverPortInput;
    public List<TablePosition> tablePositions;

    // server streaming for Game from server
    private AsyncServerStreamingCall<GameData> _stream;

    // debug
    private Grpc.Core.Logging.LogLevelFilterLogger _logger;
    public QUI_SwitchToggle devToggle;
    public QUI_SwitchToggle debugGrpcToggle;

    // On RPC error call these actions
    // private UnityAction rpcErrorAction;
    private RpcErrorEvent _mRPCErrorEvent;
    private GameFailedEvent _mGameFailedEvent;

    public override void Awake()
    {
        base.Awake();
        // EnabledGrpcTracing();
    }

    private void EnabledGrpcTracing()
    {

        Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "info");
        Environment.SetEnvironmentVariable("GRPC_DNS_RESOLVER", "native");
        Environment.SetEnvironmentVariable("GRPC_TRACE", "all");
        Debug.Log("Setting Grpc Logger");

        _logger = new Grpc.Core.Logging.LogLevelFilterLogger(new GrpcLogger(), Grpc.Core.Logging.LogLevel.Info);
        GrpcEnvironment.SetLogger(_logger);

        _logger.Debug("GRPC_VERBOSITY = " + Environment.GetEnvironmentVariable("GRPC_VERBOSITY"));
        _logger.Debug("GRPC_TRACE = " + Environment.GetEnvironmentVariable("GRPC_TRACE"));
    }

    private void Start()
    {
        _localHuman = tablePositions[3];

        _mRPCErrorEvent ??= new RpcErrorEvent();
        // TODO: Fix for all players
        _mRPCErrorEvent.AddListener(_localHuman.avatar.AnimateShrug);

        _mGameFailedEvent ??= new GameFailedEvent();
        _mGameFailedEvent.AddListener(_localHuman.avatar.AnimateDefeat);
    }


    public void Register()
    {
        
        int port = Convert.ToInt32(serverPortInput.text);
        string serverName = serverNameInput.text;
        bool insecure = false;

        if (devToggle.toggle.isOn)
        {
            port = 8443;
            serverName = "pepper-poker-grpc"; // make sure this is in /etc/hosts pointing to 127.0.0.1
            insecure = true;
        }

        try
        {
            _pokerClient = new PokerClient(serverName, port, insecure);
        }
        catch (Exception ex)
        {
            _mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        StartCoroutine(nameof(DoRegister));
    }

    public void JoinTable()
    {
        StartCoroutine(nameof(DoJoinTable));
    }
    
    private void DoRegister()
    {
        clientInfo.PlayerUsername = uiUpdater.playerUsernameInput.text;

        try
        {
            clientInfo.PlayerID = _pokerClient.Register(clientInfo);
        }
        catch (RpcException ex)
        {
            _mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        uiUpdater.playerUsernameDisplay.SetText(clientInfo.PlayerUsername);
        StartCoroutine(_localHuman.avatar.Say($"Hi there {clientInfo.PlayerUsername}!"));
        tablePositions[3].avatar.AnimateWave();
    }

    private void DoJoinTable()
    {
        Debug.Log("Joining table...");

        try
        {
            (clientInfo.TableID, game.PlayerRealPosition) = _pokerClient.JoinTable(clientInfo);
        }
        catch (RpcException ex)
        {
            _mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        Debug.Log("Table ID: " + clientInfo.TableID);
        Debug.Log("Player Position: " + game.PlayerRealPosition);

        // Kick off background refresh thread for Game
        StartInfoStream();
    }

    public void ActionAllIn()
    {
        if (!game.IsMyTurn(clientInfo.PlayerID, _lastTurnID))
        {
            _mRPCErrorEvent.Invoke("Not your turn!");
            StartCoroutine(_localHuman.avatar.Say("Not your turn!"));
            return;
        }

        try
        {
            _pokerClient.ActionAllIn(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
            _mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        _lastTurnID = game.WaitTurnNum();
    }

    public void ActionBuyIn()
    {
        try
        {
            _pokerClient.ActionBuyIn(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
            _mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        _lastTurnID = game.WaitTurnNum();
    }

    public void ActionCheck()
    {
        if (!game.IsMyTurn(clientInfo.PlayerID, _lastTurnID))
        {
            _mRPCErrorEvent.Invoke("Not your turn!");
            StartCoroutine(_localHuman.avatar.Say("Not your turn!"));
            return;
        }

        try
        {
            _pokerClient.ActionCheck(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
            _mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        _lastTurnID = game.WaitTurnNum();
    }

    public void ActionCall()
    {
        if (!game.IsMyTurn(clientInfo.PlayerID, _lastTurnID))
        {
            _mRPCErrorEvent.Invoke("Not your turn!");
            StartCoroutine(_localHuman.avatar.Say("Not your turn!"));
            return;
        }

        try
        {
            _pokerClient.ActionCall(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
            _mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        _lastTurnID = game.WaitTurnNum();
    }

    public void ActionFold()
    {
        if (!game.IsMyTurn(clientInfo.PlayerID, _lastTurnID))
        {
            _mRPCErrorEvent.Invoke("Not your turn!");
            StartCoroutine(_localHuman.avatar.Say("Not your turn!"));
            return;
        }

        try
        {
            _pokerClient.ActionFold(clientInfo);
        }
        catch (InvalidTurnException ex)
        {
            _mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        _lastTurnID = game.WaitTurnNum();
    }

    public void ActionBet()
    {
        if (!game.IsMyTurn(clientInfo.PlayerID, _lastTurnID))
        {
            _mRPCErrorEvent.Invoke("Not your turn!");
            StartCoroutine(_localHuman.avatar.Say("Not your turn!"));
            return;
        }

        long amount;
        string input = uiUpdater.betAmountInput.text;
        try
        {
            amount = Convert.ToInt64(input);
        }
        catch (FormatException ex)
        {
            _mRPCErrorEvent.Invoke($"(input: {input}) {ex}");
            return;
        }

        try
        {
            _pokerClient.ActionBet(clientInfo, amount);
        }
        catch (InvalidTurnException ex)
        {
            _mRPCErrorEvent.Invoke(ex.ToString());
            return;
        }

        _lastTurnID = game.WaitTurnNum();
    }

    private void StartInfoStream()
    {
        _stream = _pokerClient.GetGameDataStreaming(clientInfo);

        Debug.Log("Starting server stream listener...");
        StartCoroutine(nameof(StartServerStream));
    }


    // StartServerStream starts a background task listening to server responses
    // https://github.com/grpc/grpc/issues/21734#issuecomment-578519701
    private async Task<AsyncServerStreamingCall<GameData>> StartServerStream()
    {
        try
        {
            while (await _stream.ResponseStream.MoveNext())
            {
                _tokenSource.Token.ThrowIfCancellationRequested();

                GameData gd = _stream.ResponseStream.Current;
                game.Set(gd);
                game.PlayerRealPosition = gd.Player.Position;

                // Debug.Log($"> {gd}");

                AckIfNeeded(gd.Info.AckToken);
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            _mGameFailedEvent.Invoke($"Stream cancelled: {ex}");
        }
        catch (OperationCanceledException ex)
        {
            _mGameFailedEvent.Invoke($"server streaming thread cancelled: {ex}");
            _stream.Dispose();
        }
        catch (RpcException ex)
        {
            _mGameFailedEvent.Invoke(ex.ToString());
        }
        catch (Exception ex)
        {
            _mGameFailedEvent.Invoke($"Server reading thread failed: {ex}");
            Application.Quit();
        }

        Debug.Log("Exiting server info thread...");
        return null;
    }


    private void AckIfNeeded(string token)
    {
        if (_lastAckToken == token || token == "")
        {
            return;
        }

        try
        {
            _pokerClient.ActionAckToken(clientInfo, token);
        }
        catch (InvalidTurnException ex)
        {
            Debug.Log(ex.ToString());
        }

        _lastAckToken = token;
    }

    private void OnApplicationQuit()
    {
        _tokenSource.Cancel();
        StopAllCoroutines();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }
}