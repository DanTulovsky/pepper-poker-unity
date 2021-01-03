using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using Poker;
using UnityEngine;
using Newtonsoft.Json;

public class PokerClient
{
    private readonly PokerServer.PokerServerClient _client;

    // Inside StreamingAssets folder
    private readonly string _devServerCert = "server.crt";

    internal PokerClient(string serverName, int serverPort, bool insecure)
    {
        var opts = new List<ChannelOption>();
        SslCredentials sslCredentials;

        Debug.Log($"Connecting to: {serverName}:{serverPort}; insecure? {insecure}");

        var token = GetTokenFromCredentials();

        async Task AsyncAuthInterceptor(AuthInterceptorContext context, Metadata metadata)
        {
            await Task.Delay(200).ConfigureAwait(false); // make sure the operation is asynchronous.
            metadata.Add("authorization", $"Bearer {token.Result}");
        }

        switch (insecure)
        {
            case true:
            {
                string rootCertificates = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, _devServerCert));
                sslCredentials = new SslCredentials(rootCertificates);

                opts.Add(new ChannelOption("InsecureSkipVerify", "True"));
                break;
            }
            default:
                sslCredentials = new SslCredentials();
                break;
        }

        opts.Add(new ChannelOption(ChannelOptions.SslTargetNameOverride, serverName));
        ChannelCredentials channelCredentials = ChannelCredentials.Create(sslCredentials,
            CallCredentials.FromInterceptor(AsyncAuthInterceptor));
        Channel channel = new Channel(serverName, serverPort, channelCredentials, opts);
        _client = new PokerServer.PokerServerClient(channel);
    }

#pragma warning disable 1998
    private static async Task<string> GetTokenFromCredentials()
    {
        Debug.Log("Getting token...");
        string tokenUrl = $"https://login.wetsnow.com/auth/realms/wetsnow/protocol/openid-connect/token";
        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "pepper-poker-grpc.wetsnow.com",
                // ["client_secret"] = "REDACTED",
                ["grant_type"] = "password",
                ["response_type"] = "token",
                ["username"] = "mrwetsnow",
                ["password"] = "Superman9"
            })
        };

        Debug.Log("Waiting for token response...");
        using HttpClient client = new HttpClient();
        HttpResponseMessage res = client.SendAsync(req).Result;

        string json = res.Content.ReadAsStringAsync().Result;
        AuthResponse j = JsonConvert.DeserializeObject<AuthResponse>(json);
        return j.AccessToken;
    }
#pragma warning restore 1998

    // Register registers with the server and gets back a PlayerID
    internal string Register(ClientInfo clientInfo)
    {
        RegisterRequest req = new RegisterRequest {ClientInfo = clientInfo};
        RegisterResponse res;

        try
        {
            res = _client.Register(req);
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
            res = _client.JoinTable(req);
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
            var stream = _client.Play(new PlayRequest(req));
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
            _client.TakeTurn(req);
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
            _client.AckToken(req);
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
            _client.TakeTurn(req);
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
            _client.TakeTurn(req);
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
            _client.TakeTurn(req);
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
            _client.TakeTurn(req);
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
            _client.TakeTurn(req);
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