using System.Net;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Semver;
using Serilog;
using Shiki.Common.Identity;
using Shiki.Common.Result;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Request;
using UniScan.Network.Server.Handler;
using UniScan.Network.Socket;
using UniScan.Network.Util;

namespace UniScan.Network.Server;

public class ClientAttributes
{
    public static readonly AttributeKey<ClientSoftwareInfo> SoftwareInfoAttribute =
        AttributeKey<ClientSoftwareInfo>.ValueOf("software");
    
    public static readonly AttributeKey<KeyValuePair<Guid, UniScan.Server.Authentication.User>> SessionAttribute =
        AttributeKey<KeyValuePair<Guid, UniScan.Server.Authentication.User>>.ValueOf("session");
}

public class ServerSocket : ISocket
{
    public ILogger Logger { get; }

    private UniScanServerChannelInitializer _channelInitializer;
    public UniScanChannelInitializer ChannelInitializer => _channelInitializer;
    private int _port;

    private IChannel? _channel;
    private MultithreadEventLoopGroup? _masterGroup;
    private MultithreadEventLoopGroup? _workerGroup;
    
    private readonly RequestManager _requestManager = new();

    public ClientsManager ClientManager { get; } = new();

    // TODO IHostMethod
    public ServerSocket(UniScanServerChannelInitializer channelInitializer, int port)
    {
        _port = port;
        
        this._channelInitializer = channelInitializer;
        this.Logger = Log.ForContext<ServerSocket>();
    }
    
    public async Task StartAsync()
    {
        _masterGroup = new MultithreadEventLoopGroup(1);
        _workerGroup = new MultithreadEventLoopGroup();

        // TODO I need websocket server too for web
        //I doubt I did enough abstraction for it yet
        try
        {
            ServerBootstrap bootstrap = new();

            bootstrap.Group(_masterGroup, _workerGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .ChildHandler(new ActionChannelInitializer<IChannel>((channel) =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    
                    pipeline.AddLast(ChannelInitializer);

                    pipeline.AddFirst(new ConnectionStateTracker());
                    pipeline.AddFirst(ClientManager);
                    pipeline.AddLast(new ResponseHandler(_requestManager));
                }));
            
            _channel = await bootstrap.BindAsync(new IPEndPoint(IPAddress.Any, _port));
            Logger.Information("Listening on port {Port}...", _port);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to start server");
            await StopAsync();
        }
    }
    
    public async Task StopAsync()
    {
        if (_requestManager != null) await _requestManager.RejectAllAsync(new OperationCanceledException("Socket is shutting down")).ContinueWith(_ => _requestManager.DisposeAsync());
        if (_channel != null) await _channel.CloseAsync();
        if (_masterGroup != null) await _masterGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        if (_workerGroup != null) await _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
    }

    public async Task<bool> SendPacketAsync(IChannel? client, IPacket packet)
    {
        if (client is not { Active: true }) return false;

        await client.WriteAndFlushAsync(packet);
        return true;
    }
    
    public async Task<Result<TResponse, Exception>> SendRequestAsync<TResponse>(IChannel? client, IRequestPayloadPart<TResponse> packet, CancellationToken ct = default)
        where TResponse : IPacket, IResponsePayloadPart
    {
        return await _requestManager.MakeRequestAsync(client, packet, ct);
    }
}