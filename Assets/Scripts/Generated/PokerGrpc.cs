// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: poker.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Poker {
  public static partial class PokerServer
  {
    static readonly string __ServiceName = "poker.PokerServer";

    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    static readonly grpc::Marshaller<global::Poker.SayHelloRequest> __Marshaller_poker_SayHelloRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.SayHelloRequest.Parser));
    static readonly grpc::Marshaller<global::Poker.SayHelloResponse> __Marshaller_poker_SayHelloResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.SayHelloResponse.Parser));
    static readonly grpc::Marshaller<global::Poker.ListTablesRequest> __Marshaller_poker_ListTablesRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.ListTablesRequest.Parser));
    static readonly grpc::Marshaller<global::Poker.ListTablesResponse> __Marshaller_poker_ListTablesResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.ListTablesResponse.Parser));
    static readonly grpc::Marshaller<global::Poker.CreateTableRequest> __Marshaller_poker_CreateTableRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.CreateTableRequest.Parser));
    static readonly grpc::Marshaller<global::Poker.CreateTableResponse> __Marshaller_poker_CreateTableResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.CreateTableResponse.Parser));
    static readonly grpc::Marshaller<global::Poker.JoinTableRequest> __Marshaller_poker_JoinTableRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.JoinTableRequest.Parser));
    static readonly grpc::Marshaller<global::Poker.JoinTableResponse> __Marshaller_poker_JoinTableResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.JoinTableResponse.Parser));
    static readonly grpc::Marshaller<global::Poker.PlayerActionRequest> __Marshaller_poker_PlayerActionRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.PlayerActionRequest.Parser));
    static readonly grpc::Marshaller<global::Poker.PlayerActionResponse> __Marshaller_poker_PlayerActionResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.PlayerActionResponse.Parser));
    static readonly grpc::Marshaller<global::Poker.GetInfoRequest> __Marshaller_poker_GetInfoRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.GetInfoRequest.Parser));
    static readonly grpc::Marshaller<global::Poker.TableInfo> __Marshaller_poker_TableInfo = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.TableInfo.Parser));
    static readonly grpc::Marshaller<global::Poker.StartHandRequest> __Marshaller_poker_StartHandRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.StartHandRequest.Parser));
    static readonly grpc::Marshaller<global::Poker.StartHandResponse> __Marshaller_poker_StartHandResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Poker.StartHandResponse.Parser));

    static readonly grpc::Method<global::Poker.SayHelloRequest, global::Poker.SayHelloResponse> __Method_SayHello = new grpc::Method<global::Poker.SayHelloRequest, global::Poker.SayHelloResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "SayHello",
        __Marshaller_poker_SayHelloRequest,
        __Marshaller_poker_SayHelloResponse);

    static readonly grpc::Method<global::Poker.ListTablesRequest, global::Poker.ListTablesResponse> __Method_ListTables = new grpc::Method<global::Poker.ListTablesRequest, global::Poker.ListTablesResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "ListTables",
        __Marshaller_poker_ListTablesRequest,
        __Marshaller_poker_ListTablesResponse);

    static readonly grpc::Method<global::Poker.CreateTableRequest, global::Poker.CreateTableResponse> __Method_CreateTable = new grpc::Method<global::Poker.CreateTableRequest, global::Poker.CreateTableResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "CreateTable",
        __Marshaller_poker_CreateTableRequest,
        __Marshaller_poker_CreateTableResponse);

    static readonly grpc::Method<global::Poker.JoinTableRequest, global::Poker.JoinTableResponse> __Method_JoinTable = new grpc::Method<global::Poker.JoinTableRequest, global::Poker.JoinTableResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "JoinTable",
        __Marshaller_poker_JoinTableRequest,
        __Marshaller_poker_JoinTableResponse);

    static readonly grpc::Method<global::Poker.PlayerActionRequest, global::Poker.PlayerActionResponse> __Method_TakeTurn = new grpc::Method<global::Poker.PlayerActionRequest, global::Poker.PlayerActionResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "TakeTurn",
        __Marshaller_poker_PlayerActionRequest,
        __Marshaller_poker_PlayerActionResponse);

    static readonly grpc::Method<global::Poker.GetInfoRequest, global::Poker.TableInfo> __Method_GetGameInfo = new grpc::Method<global::Poker.GetInfoRequest, global::Poker.TableInfo>(
        grpc::MethodType.DuplexStreaming,
        __ServiceName,
        "GetGameInfo",
        __Marshaller_poker_GetInfoRequest,
        __Marshaller_poker_TableInfo);

    static readonly grpc::Method<global::Poker.StartHandRequest, global::Poker.StartHandResponse> __Method_StartHand = new grpc::Method<global::Poker.StartHandRequest, global::Poker.StartHandResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "StartHand",
        __Marshaller_poker_StartHandRequest,
        __Marshaller_poker_StartHandResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Poker.PokerReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of PokerServer</summary>
    [grpc::BindServiceMethod(typeof(PokerServer), "BindService")]
    public abstract partial class PokerServerBase
    {
      public virtual global::System.Threading.Tasks.Task<global::Poker.SayHelloResponse> SayHello(global::Poker.SayHelloRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Poker.ListTablesResponse> ListTables(global::Poker.ListTablesRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Poker.CreateTableResponse> CreateTable(global::Poker.CreateTableRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Poker.JoinTableResponse> JoinTable(global::Poker.JoinTableRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Poker.PlayerActionResponse> TakeTurn(global::Poker.PlayerActionRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task GetGameInfo(grpc::IAsyncStreamReader<global::Poker.GetInfoRequest> requestStream, grpc::IServerStreamWriter<global::Poker.TableInfo> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Poker.StartHandResponse> StartHand(global::Poker.StartHandRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for PokerServer</summary>
    public partial class PokerServerClient : grpc::ClientBase<PokerServerClient>
    {
      /// <summary>Creates a new client for PokerServer</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public PokerServerClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for PokerServer that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public PokerServerClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected PokerServerClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected PokerServerClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::Poker.SayHelloResponse SayHello(global::Poker.SayHelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHello(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Poker.SayHelloResponse SayHello(global::Poker.SayHelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_SayHello, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Poker.SayHelloResponse> SayHelloAsync(global::Poker.SayHelloRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SayHelloAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Poker.SayHelloResponse> SayHelloAsync(global::Poker.SayHelloRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_SayHello, null, options, request);
      }
      public virtual global::Poker.ListTablesResponse ListTables(global::Poker.ListTablesRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ListTables(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Poker.ListTablesResponse ListTables(global::Poker.ListTablesRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_ListTables, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Poker.ListTablesResponse> ListTablesAsync(global::Poker.ListTablesRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ListTablesAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Poker.ListTablesResponse> ListTablesAsync(global::Poker.ListTablesRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_ListTables, null, options, request);
      }
      public virtual global::Poker.CreateTableResponse CreateTable(global::Poker.CreateTableRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return CreateTable(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Poker.CreateTableResponse CreateTable(global::Poker.CreateTableRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_CreateTable, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Poker.CreateTableResponse> CreateTableAsync(global::Poker.CreateTableRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return CreateTableAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Poker.CreateTableResponse> CreateTableAsync(global::Poker.CreateTableRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_CreateTable, null, options, request);
      }
      public virtual global::Poker.JoinTableResponse JoinTable(global::Poker.JoinTableRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return JoinTable(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Poker.JoinTableResponse JoinTable(global::Poker.JoinTableRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_JoinTable, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Poker.JoinTableResponse> JoinTableAsync(global::Poker.JoinTableRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return JoinTableAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Poker.JoinTableResponse> JoinTableAsync(global::Poker.JoinTableRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_JoinTable, null, options, request);
      }
      public virtual global::Poker.PlayerActionResponse TakeTurn(global::Poker.PlayerActionRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return TakeTurn(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Poker.PlayerActionResponse TakeTurn(global::Poker.PlayerActionRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_TakeTurn, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Poker.PlayerActionResponse> TakeTurnAsync(global::Poker.PlayerActionRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return TakeTurnAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Poker.PlayerActionResponse> TakeTurnAsync(global::Poker.PlayerActionRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_TakeTurn, null, options, request);
      }
      public virtual grpc::AsyncDuplexStreamingCall<global::Poker.GetInfoRequest, global::Poker.TableInfo> GetGameInfo(grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetGameInfo(new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncDuplexStreamingCall<global::Poker.GetInfoRequest, global::Poker.TableInfo> GetGameInfo(grpc::CallOptions options)
      {
        return CallInvoker.AsyncDuplexStreamingCall(__Method_GetGameInfo, null, options);
      }
      public virtual global::Poker.StartHandResponse StartHand(global::Poker.StartHandRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return StartHand(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Poker.StartHandResponse StartHand(global::Poker.StartHandRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_StartHand, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Poker.StartHandResponse> StartHandAsync(global::Poker.StartHandRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return StartHandAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Poker.StartHandResponse> StartHandAsync(global::Poker.StartHandRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_StartHand, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override PokerServerClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new PokerServerClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(PokerServerBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_SayHello, serviceImpl.SayHello)
          .AddMethod(__Method_ListTables, serviceImpl.ListTables)
          .AddMethod(__Method_CreateTable, serviceImpl.CreateTable)
          .AddMethod(__Method_JoinTable, serviceImpl.JoinTable)
          .AddMethod(__Method_TakeTurn, serviceImpl.TakeTurn)
          .AddMethod(__Method_GetGameInfo, serviceImpl.GetGameInfo)
          .AddMethod(__Method_StartHand, serviceImpl.StartHand).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static void BindService(grpc::ServiceBinderBase serviceBinder, PokerServerBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_SayHello, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Poker.SayHelloRequest, global::Poker.SayHelloResponse>(serviceImpl.SayHello));
      serviceBinder.AddMethod(__Method_ListTables, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Poker.ListTablesRequest, global::Poker.ListTablesResponse>(serviceImpl.ListTables));
      serviceBinder.AddMethod(__Method_CreateTable, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Poker.CreateTableRequest, global::Poker.CreateTableResponse>(serviceImpl.CreateTable));
      serviceBinder.AddMethod(__Method_JoinTable, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Poker.JoinTableRequest, global::Poker.JoinTableResponse>(serviceImpl.JoinTable));
      serviceBinder.AddMethod(__Method_TakeTurn, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Poker.PlayerActionRequest, global::Poker.PlayerActionResponse>(serviceImpl.TakeTurn));
      serviceBinder.AddMethod(__Method_GetGameInfo, serviceImpl == null ? null : new grpc::DuplexStreamingServerMethod<global::Poker.GetInfoRequest, global::Poker.TableInfo>(serviceImpl.GetGameInfo));
      serviceBinder.AddMethod(__Method_StartHand, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Poker.StartHandRequest, global::Poker.StartHandResponse>(serviceImpl.StartHand));
    }

  }
}
#endregion
