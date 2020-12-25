using System;
using System.Collections.Generic;
using System.IO;
using Grpc.Core;
using Poker;
using UnityEngine;

public class PokerClient
{
    private readonly PokerServer.PokerServerClient client;
    private readonly string devServerCert = "server.crt";

    internal PokerClient(string serverName, int serverPort, bool insecure)
    {
        SslCredentials credentials;
        List<ChannelOption> opts = new List<ChannelOption>();
        Channel channel;

        Debug.Log($"Connecting to: {serverName}:{serverPort}; insecure? {insecure}");

        if (insecure)
        {
            string rootCertificates = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, devServerCert));
            credentials = new SslCredentials(rootCertificates);

            opts.Add(new ChannelOption("InsecureSkipVerify", "True"));
            opts.Add(new ChannelOption(ChannelOptions.SslTargetNameOverride, "pepper-poker"));
            
            channel = new Channel(serverName, serverPort, credentials, opts);
        }
        else
        {
            credentials = new SslCredentials();
            opts.Add(new ChannelOption(ChannelOptions.SslTargetNameOverride, serverName));
            channel = new Channel(serverName, serverPort, credentials, opts);
        }

        client = new PokerServer.PokerServerClient(channel);
    }


    // Register registers with the server and gets back a PlayerID
    internal string Register(ClientInfo clientInfo)
    {
        RegisterRequest req = new RegisterRequest {ClientInfo = clientInfo};
        RegisterResponse res;

        try
        {
            res = client.Register(req);
        }
        catch (RpcException ex)
        {
            Debug.Log(ex.ToString());
            throw;
        }

        return res.PlayerID;
    }

    internal (string id, long position) JoinTable(ClientInfo clientInfo)
    {
        Debug.Log($"{clientInfo.PlayerUsername} Calling ActionJoinTable");
        JoinTableRequest req = new JoinTableRequest
        {
            ClientInfo = clientInfo,
        };
        JoinTableResponse res;

        try
        {
            res = client.JoinTable(req);
        }
        catch (RpcException ex)
        {
            Debug.Log(ex.ToString());
            throw;
        }

        return (res.TableID, res.Position);
    }

    // Stream version
    internal AsyncServerStreamingCall<GameData> GetGameDataStreaming(ClientInfo clientInfo)
    {
        try
        {
            PlayRequest req = new PlayRequest
            {
                ClientInfo = clientInfo,
            };
            var stream = client.Play(new PlayRequest(req));
            return stream;
        }
        catch (RpcException ex)
        {
            Debug.Log(ex.ToString());
            throw;
        }
    }

    // ActionBet used for Raise
    internal void ActionBet(ClientInfo clientInfo, long amount)
    {
        Debug.Log($"{clientInfo.PlayerUsername} Calling ActionBet ({amount})");
        TakeTurnRequest req = new TakeTurnRequest
        {
            ClientInfo = clientInfo,
            PlayerAction = PlayerAction.Bet,
            ActionOpts = new ActionOpts {BetAmount = amount}
        };

        try
        {
            client.TakeTurn(req);
        }
        catch (RpcException ex)
        {
            throw new InvalidTurnException(ex.ToString());
        }
    }

    // ActionAckToken acks a token
    internal void ActionAckToken(ClientInfo clientInfo, string token)
    {
        Debug.Log($"{clientInfo.PlayerUsername} Calling ActionAck");
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
    internal void ActionAllIn(ClientInfo clientInfo)
    {
        Debug.Log($"{clientInfo.PlayerUsername} Calling ActionAllIn");
        TakeTurnRequest req = new TakeTurnRequest
        {
            ClientInfo = clientInfo,
            PlayerAction = PlayerAction.AllIn,
        };

        try
        {
            client.TakeTurn(req);
        }
        catch (RpcException ex)
        {
            throw new InvalidTurnException(ex.ToString());
        }
    }


    // ActionBuyIn adds more money to the stack
    internal void ActionBuyIn(ClientInfo clientInfo)
    {
        Debug.Log($"{clientInfo.PlayerUsername} Calling ActionBuyIn");
        TakeTurnRequest req = new TakeTurnRequest
        {
            ClientInfo = clientInfo,
            PlayerAction = PlayerAction.BuyIn,
        };

        try
        {
            client.TakeTurn(req);
        }
        catch (RpcException ex)
        {
            throw new InvalidTurnException(ex.ToString());
        }
    }

    // ActionCall call
    internal void ActionCall(ClientInfo clientInfo)
    {
        Debug.Log($"{clientInfo.PlayerUsername} Calling ActionCall");
        TakeTurnRequest req = new TakeTurnRequest
        {
            ClientInfo = clientInfo,
            PlayerAction = PlayerAction.Call,
        };

        try
        {
            client.TakeTurn(req);
        }
        catch (RpcException ex)
        {
            throw new InvalidTurnException(ex.ToString());
        }
    }

    internal void ActionCheck(ClientInfo clientInfo)
    {
        Debug.Log($"{clientInfo.PlayerUsername} Calling ActionCheck");
        TakeTurnRequest req = new TakeTurnRequest
        {
            ClientInfo = clientInfo,
            PlayerAction = PlayerAction.Check,
        };

        try
        {
            client.TakeTurn(req);
        }
        catch (RpcException ex)
        {
            throw new InvalidTurnException(ex.ToString());
        }
    }

    internal void ActionFold(ClientInfo clientInfo)
    {
        Debug.Log($"{clientInfo.PlayerUsername} Calling ActionFold");
        TakeTurnRequest req = new TakeTurnRequest
        {
            ClientInfo = clientInfo,
            PlayerAction = PlayerAction.Fold,
        };

        try
        {
            client.TakeTurn(req);
        }
        catch (RpcException ex)
        {
            throw new InvalidTurnException(ex.ToString());
        }
    }
}

public class InvalidTurnException : Exception
{
    public InvalidTurnException()
    {
    }

    public InvalidTurnException(string message)
        : base(message)
    {
    }

    public InvalidTurnException(string message, Exception inner)
        : base(message, inner)
    {
    }
}