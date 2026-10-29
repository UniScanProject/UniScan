using System.Data;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Serilog;
using Shiki.Common.Result;
using UniScan.Network.Client.Extensions;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Group;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Request;
using UniScan.Network.Socket;
using UniScan.Network.Util;

namespace UniScan.Network.Client;

public class ClientSocket : ISocket, IRequestTransport
{
    public ILogger Logger => Log.ForContext<ClientSocket>();
    
    private readonly INetworkGroupManager _networkGroupManager;

    private readonly UniScanClientChannelInitializer _channelInitializer;
    public UniScanChannelInitializer ChannelInitializer => _channelInitializer;
    public ConnectionStateTracker ConnectionState { get; } = new();

    private readonly IRemoteConnectionMethod _connectionMethod;
    public IChannel? Channel { get; private set; }

    public bool Connected => Channel?.Active == true;

    private readonly RequestManager _requestManager = new();

    public ClientSocket(UniScanClientChannelInitializer channelInitializer, IRemoteConnectionMethod connectionMethod, INetworkGroupManager groupManager)
    {
        ArgumentNullException.ThrowIfNull(channelInitializer);
        ArgumentNullException.ThrowIfNull(connectionMethod);

        _connectionMethod = connectionMethod;
        _channelInitializer = channelInitializer;
        _networkGroupManager = groupManager;
    }

    public async Task StartAsync()
    {
        if (_connectionMethod is null) throw new NullReferenceException(nameof(_connectionMethod));
        
        try
        {
            Bootstrap bs = new Bootstrap()
                                          .ConnectionMethod(_connectionMethod, _networkGroupManager)
                                          .Handler(new ActionChannelInitializer<IChannel>((channel) =>
                                           {
                                               IChannelPipeline pipeline = channel.Pipeline;

                                               _connectionMethod.ApplyPipeline(pipeline);
                                               
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