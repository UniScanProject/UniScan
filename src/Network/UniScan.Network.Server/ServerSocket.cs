using System.Collections.Immutable;
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
using UniScan.Network.Group;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Request;
using UniScan.Network.Server.Handler;
using UniScan.Network.Server.HostMethod;
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

    private readonly ImmutableList<IServerHostMethod> _hostMethods;
    private readonly List<IChannel> _channels = [];

    private readonly IHostNetworkGroupManager _networkGroupManager;
    
    private readonly RequestManager _requestManager = new();

    public ClientsManager ClientManager { get; } = new();

    public ServerSocket(UniScanServerChannelInitializer channelInitializer, IEnumerable<IServerHostMethod> methods, IHostNetworkGroupManager groupManager)
    {
        _hostMethods = [.. methods];
        _channelInitializer = channelInitializer;
        _networkGroupManager = groupManager;
        
        Logger = Log.ForContext<ServerSocket>();
    }
    
    public async Task StartAsync()
    {
        if (!_hostMethods.Any())
        {
            throw new Exception("No host methods found");
        }
        
        try
        {
            var task = _hostMethods.Select(async m =>
            {
                ServerBootstrap bootstrap = new();
                m.Apply(bootstrap, _networkGroupManager);
                
                bootstrap.ChildHandler(new ActionChannelInitializer<IChannel>((channel) =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    
                    m.ApplyPipeline(pipeline);
                    
                    pipeline.AddLast(ChannelInitializer);

                    pipeline.AddFirst(new ConnectionStateTracker());
                    pipeline.AddFirst(ClientManager);
                    pipeline.AddLast(new ResponseHandler(_requestManager));
                }));

                _channels.Add(await m.BindAsync(bootstrap));
                
                Logger.Information("Listening on transport {Transport}", m.ToDisplayString());
            });

            await Task.WhenAll(task);
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

        var t = _channels.Select(c => c.CloseAsync());
        await Task.WhenAll(t);
        
        _channels.Clear();
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