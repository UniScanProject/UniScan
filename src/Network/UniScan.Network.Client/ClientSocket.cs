using System.Data;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Serilog;
using Shiki.Common.Result;
using UniScan.Network.Client.Extensions;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;
using UniScan.Network.Request;
using UniScan.Network.Socket;
using UniScan.Network.Socket.Configuration;
using UniScan.Network.Util;

namespace UniScan.Network.Client;

public interface IClientSocketFactory
{
    ClientSocket CreateInstance(IRemoteConnectionMethod connectionMethod);
}

public class ClientSocketFactory(
    PacketRegistry registry,
    IEnumerable<IPipelineConfigurator> configurators,
    IServiceProvider provider
) : IClientSocketFactory
{
    public ClientSocket CreateInstance(IRemoteConnectionMethod connectionMethod) => new(new UniScanClientChannelInitializer(registry, configurators, provider), connectionMethod);
}

public class ClientSocket : ISocket
{
    public ILogger Logger => Log.ForContext<ClientSocket>();
    private MultithreadEventLoopGroup? _group;

    private readonly UniScanClientChannelInitializer _channelInitializer;
    public UniScanChannelInitializer ChannelInitializer => _channelInitializer;
    public ConnectionStateTracker ConnectionState { get; } = new();

    private readonly IRemoteConnectionMethod _connectionMethod;
    public IChannel? Channel { get; private set; }

    public bool Connected => Channel?.Active == true;

    private readonly RequestManager _requestManager = new();

    public ClientSocket(UniScanClientChannelInitializer channelInitializer, IRemoteConnectionMethod connectionMethod)
    {
        ArgumentNullException.ThrowIfNull(channelInitializer);
        ArgumentNullException.ThrowIfNull(connectionMethod);

        _connectionMethod = connectionMethod;
        _channelInitializer = channelInitializer;
    }

    public async Task StartAsync()
    {
        if (_connectionMethod is null) throw new NullReferenceException(nameof(_connectionMethod));

        _group ??= new MultithreadEventLoopGroup();

        try
        {
            Bootstrap bs = new Bootstrap().Group(_group)
                                          .ConnectionMethod(_connectionMethod)
                                          .Handler(new ActionChannelInitializer<IChannel>((channel) =>
                                           {
                                               IChannelPipeline pipeline = channel.Pipeline;

                                               pipeline.AddFirst(ConnectionState);

                                               pipeline.AddLast(ChannelInitializer);
                                               pipeline.AddLast(new ResponseHandler(_requestManager));
                                           }));

            Channel = await _connectionMethod.ConnectAsync(bs);
        }
        catch (Exception)
        {
            await StopAsync();
            throw;
        }
    }

    public async Task StopAsync()
    {
        if (_requestManager != null)
            await _requestManager.RejectAllAsync(new OperationCanceledException("Socket is shutting down"));
        if (Channel != null)
        {
            await Channel.CloseAsync();
            Channel = null;
        }

        if (_group != null)
        {
            await _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            _group = null;
        }
    }

    public async Task<bool> SendPacketAsync(IPacket packet) => await SendPacketAsync(Channel, packet);

    public async Task<bool> SendPacketAsync(IChannel? channel, IPacket packet)
    {
        if (channel is not { Active: true }) return false;

        await channel.WriteAndFlushAsync(packet);
        return true;
    }

    public async Task<Result<TResponse, Exception>> SendRequestAsync<TResponse>(
        IRequestPayloadPart<TResponse> request, CancellationToken ct = default)
        where TResponse : IPacket, IResponsePayloadPart => await _requestManager.MakeRequestAsync(Channel, request, ct);

    public async Task<Result<TResponse, Exception>> SendRequestAsync<TResponse>(
        IChannel? channel, IRequestPayloadPart<TResponse> request, CancellationToken ct = default)
        where TResponse : IPacket, IResponsePayloadPart
    {
        if (channel is not { Active: true })
            return new Result<TResponse, Exception>(new ArgumentNullException(nameof(channel)));

        return await _requestManager.MakeRequestAsync(channel, request, ct);
    }
}