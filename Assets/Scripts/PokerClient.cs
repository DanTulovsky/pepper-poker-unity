using System;
using System.Collections.Generic;
using System.IO;
using Grpc.Core;
using Poker;
using UnityEngine;

public class PokerClient {
    private readonly PokerServer.PokerServerClient client;
    private const string Server = "montester";
    private const int Port = 8443;

    internal PokerClient() {
        string rootCertificates = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Server.crt"));
        SslCredentials credentials = new SslCredentials(rootCertificates);

        var opts = new List<ChannelOption> { new ChannelOption("InsecureSkipVerify", "True"), new ChannelOption(ChannelOptions.SslTargetNameOverride, "montester") };
        Channel channel = new Channel(Server, Port, credentials, opts);
        client = new PokerServer.PokerServerClient(channel);
    }

    
    // Register registers with the server and gets back a PlayerID
    internal string Register(ClientInfo clientInfo) {
        RegisterRequest req = new RegisterRequest { ClientInfo = clientInfo };
        RegisterResponse res;

        try {
            res = client.Register(req);
        } catch (RpcException ex) {
            Debug.Log(ex.ToString());
            throw;
        }

        return res.PlayerID;
    }

    internal (string id, long position) JoinTable(ClientInfo clientInfo) {
        Debug.Log("Calling JoinTable");
        JoinTableRequest req = new JoinTableRequest {
            ClientInfo = clientInfo,
        };
        JoinTableResponse res;

        try {
            res = client.JoinTable(req);
        } catch (RpcException ex) {
            Debug.Log(ex.ToString());
            throw;
        }

        return (res.TableID, res.Position);
    }

    // Stream version
    internal AsyncServerStreamingCall<Poker.GameData> GetGameDataStreaming(ClientInfo clientInfo) {
        try
        {
            PlayRequest req = new PlayRequest
            {
                ClientInfo = clientInfo,
            };
            var stream = client.Play(new PlayRequest(req));
            return stream;
        } catch (RpcException ex) {
            Debug.Log(ex.ToString());
            throw;
        }
    }

    // ActionBet used for Raise
    internal void ActionBet(ClientInfo clientInfo, long amount) {
        Debug.Log("Calling ActionBet: " + amount);
        TakeTurnRequest req = new TakeTurnRequest
        {
            ClientInfo = clientInfo,
            PlayerAction = PlayerAction.Bet,
            ActionOpts = new ActionOpts { BetAmount = amount}
        };

        try {
            client.TakeTurn(req);
        } catch (RpcException ex) {
            throw new InvalidTurnException(ex.ToString());
        }
    }

    // ActionAckToken acks a token
    internal void ActionAckToken(ClientInfo clientInfo, string token) {
        Debug.Log("Calling ActionAck");
        AckTokenRequest req = new AckTokenRequest
        {
            ClientInfo = clientInfo,
            Token = token,
        };

        try
        {
            client.AckToken(req);
        }
        catch (RpcException ex)
        {
            throw new InvalidTurnException(ex.ToString());
        }
    }
    
    // ActionAllIn goes all in
    internal void ActionAllIn(ClientInfo clientInfo) {
        Debug.Log("Calling ActionAllIn");
        TakeTurnRequest req = new TakeTurnRequest
        {
            ClientInfo = clientInfo,
            PlayerAction = PlayerAction.AllIn,
        };

        try {
            client.TakeTurn(req);
        } catch (RpcException ex) {
            throw new InvalidTurnException(ex.ToString());
        }
    }
    
    
    // ActionBuyIn adds more money to the stack
    internal void ActionBuyIn(ClientInfo clientInfo) {
        Debug.Log("Calling ActionBuyIn");
        TakeTurnRequest req = new TakeTurnRequest
        {
            ClientInfo = clientInfo,
            PlayerAction = PlayerAction.BuyIn,
        };

        try {
            client.TakeTurn(req);
        } catch (RpcException ex) {
            throw new InvalidTurnException(ex.ToString());
        }
    }
    
    // ActionCall call
    internal void ActionCall(ClientInfo clientInfo) {
        Debug.Log("Calling ActionCall");
        TakeTurnRequest req = new TakeTurnRequest
        {
            ClientInfo = clientInfo,
            PlayerAction = PlayerAction.Call,
        };

        try {
            client.TakeTurn(req);
        } catch (RpcException ex) {
            throw new InvalidTurnException(ex.ToString());
        }
    }

    internal void ActionCheck(ClientInfo clientInfo) {
        Debug.Log("Calling ActionCheck");
        TakeTurnRequest req = new TakeTurnRequest
        {
            ClientInfo = clientInfo,
            PlayerAction = PlayerAction.Check,
        };

        try {
            client.TakeTurn(req);
        } catch (RpcException ex) {
            throw new InvalidTurnException(ex.ToString());
        }
    }

    internal void ActionFold(ClientInfo clientInfo) {
        Debug.Log("Calling ActionFold");
        TakeTurnRequest req = new TakeTurnRequest
        {
            ClientInfo = clientInfo,
            PlayerAction = PlayerAction.Fold,
        };

        try {
            client.TakeTurn(req);
        } catch (RpcException ex) {
            throw new InvalidTurnException(ex.ToString());
        }
    }
}
public class InvalidTurnException : Exception {
    public InvalidTurnException() {
    }

    public InvalidTurnException(string message)
        : base(message) {
    }

    public InvalidTurnException(string message, Exception inner)
        : base(message, inner) {
    }
}
