using System;
using Grpc.Core;
using Poker;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Action = Poker.Action;


public class PokerClient {
    private readonly PokerServer.PokerServerClient client;
    private readonly Channel channel;
    private readonly string server = "montester";
    private readonly int port = 8443;

    internal PokerClient() {
        var rootCertificates = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Server.crt"));
        var credentials = new SslCredentials(rootCertificates);

        var opts = new List<ChannelOption> { new ChannelOption("InsecureSkipVerify", "True"), new ChannelOption(ChannelOptions.SslTargetNameOverride, "montester") };
        channel = new Channel(server, port, credentials, opts);
        client = new PokerServer.PokerServerClient(channel);
    }

    // SayHello registers with the server and gets back a PlayerID
    internal string SayHello(string name) {
        var req = new SayHelloRequest { Name = name };
        var res = new SayHelloResponse { };

        try {
            res = client.SayHello(req);
        } catch (RpcException ex) {
            Debug.Log(ex.ToString());
        }

        return res.PlayerID;
    }

    internal (string id, long position) JoinTable(string tableID, string playerID) {
        Debug.Log("Calling JoinTable");
        var req = new JoinTableRequest {
            PlayerID = playerID,
            TableID = tableID,
        };
        var res = new JoinTableResponse { };

        try {
            res = client.JoinTable(req);
        } catch (RpcException ex) {
            Debug.Log(ex.ToString());
        }

        return (res.TableID, res.Position);
    }

    // Stream version
    internal AsyncDuplexStreamingCall<GetInfoRequest, Poker.TableInfo> GetInfoStreaming() {

        AsyncDuplexStreamingCall<GetInfoRequest, Poker.TableInfo> stream;

        try {
            stream = client.GetGameInfo();
            return stream;
        } catch (RpcException ex) {
            Debug.Log(ex.ToString());
        }

        return null;
    }

    // ActionBet used for Raise, AllIn
    internal void ActionBet(string tableID, string playerID, string roundID, long amount) {
        Debug.Log("Calling ActionBet: " + amount);
        var req = new PlayerActionRequest {
            PlayerID = playerID,
            TableID = tableID,
            RoundID = roundID,
            Action = Action.Bet,
            Opts = new ActionOpts { BetAmount = amount },
        };
        PlayerActionResponse res;

        try {
            res = client.TakeTurn(req);
        } catch (RpcException ex) {
            Debug.Log(ex.ToString());
        }
    }

    // ActionCall calls
    internal void ActionCall(string tableID, string playerID, string roundID) {
        Debug.Log("Calling ActionCall");
        var req = new PlayerActionRequest {
            PlayerID = playerID,
            TableID = tableID,
            RoundID = roundID,
            Action = Action.Call,
        };
        PlayerActionResponse res;

        try {
            res = client.TakeTurn(req);
        } catch (RpcException ex) {
            Debug.Log(ex.ToString());
        }
    }

    internal void ActionCheck(string tableID, string playerID, string roundID) {
        Debug.Log("Calling ActionCheck");
        var req = new PlayerActionRequest {
            PlayerID = playerID,
            TableID = tableID,
            RoundID = roundID,
            Action = Action.Check,
        };
        PlayerActionResponse res;

        try {
            res = client.TakeTurn(req);
        } catch (RpcException ex) {
            Debug.Log(ex.ToString());
        }
    }

    internal void ActionFold(string tableID, string playerID, string roundID) {
        Debug.Log("Calling ActionFold");
        var req = new PlayerActionRequest {
            PlayerID = playerID,
            TableID = tableID,
            RoundID = roundID,
            Action = Action.Fold,
        };
        PlayerActionResponse res;

        try {
            res = client.TakeTurn(req);
        } catch (RpcException ex) {
            Debug.Log(ex.ToString());
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
