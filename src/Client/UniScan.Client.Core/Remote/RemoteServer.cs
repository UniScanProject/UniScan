using DotNetty.Common.Utilities;
using ObservableCollections;
using R3;
using Serilog;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Client.Core.Config.Remote;
using UniScan.Client.Core.Remote.Connection;
using UniScan.Client.Core.Remote.Connection.Status;
using UniScan.Client.Core.Remote.Device;
using UniScan.Network;
using UniScan.Network.Client;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Data;
using UniScan.Network.Data.Info.Remote;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.Core.Remote;

public class ServerAttributes
{
    public static readonly AttributeKey<ServerSoftwareInfo> SoftwareInfoAttribute =
        AttributeKey<ServerSoftwareInfo>.ValueOf("software");
    
    public static readonly AttributeKey<RemoteInfo> RemoteInfoAttribute =
        AttributeKey<RemoteInfo>.ValueOf("remote_info");
    
    public static readonly AttributeKey<RemoteServer> ServerAttribute =
        AttributeKey<RemoteServer>.ValueOf("server");
}

public interface IRemoteServerMutationProxy
{
    void SetSoftwareInfo(ServerSoftwareInfo softwareInfo);

    void SetRemoteInfo(RemoteInfo info);
    
    void SetConnectionStatus(IConnectionStatusContext context);
}

public class RemoteServer : IRemoteServerMutationProxy
{
    public IReadOnlyBindableReactiveProperty<string> DisplayName { get; }
    
    public Guid Id { get; }
    
    /// <summary>
    /// connection method
    /// </summary>
    public IRemoteConnectionMethod ConnectionMethod { get; }
    
    /// <summary>
    /// list of devices
    /// </summary>
    public ObservableDictionary<Slug<SnakeSlugFormatter>, RemoteDevice> Devices { get; } = [];
    
    public ServerSoftwareInfo? SoftwareInfo { get; private set; }
    
    private readonly ReactiveProperty<RemoteInfo?> _remoteInfo = new();
    public ReadOnlyReactiveProperty<RemoteInfo?> RemoteInfo => _remoteInfo;
    
    public ClientSocket Socket { get; }

    private readonly BindableReactiveProperty<IConnectionStatusContext> _connectionStatus = new(new DefaultConnectionStatusContext(ConnectionState.NotConnected));
    public IReadOnlyBindableReactiveProperty<IConnectionStatusContext> ConnectionStatus => _connectionStatus;
    
    public RemoteServer(Guid id, IRemoteConnectionMethod connectionMethod, IClientSocketFactory socketFactory)
    {
        Id = id;
        
        ConnectionMethod = connectionMethod;
        DisplayName = _remoteInfo.Select(info => info?.DisplayName ?? ConnectionMethod.ToDisplayString())
                                 .ToReadOnlyBindableReactiveProperty(ConnectionMethod.ToDisplayString());


        Socket = socketFactory.CreateInstance(connectionMethod);
        
        Socket.ConnectionState.Disconnected += (sender, args) =>
        {
            foreach (var device in Devices)
            {
                device.Value.Dispose();
            }
            Devices.Clear();

            if (ConnectionStatus.Value.State < ConnectionState.NotConnected)
            {
                _connectionStatus.Value = new DefaultConnectionStatusContext(ConnectionState.NotConnected);
            }
            
            Log.Information("Disconnected from {RemoteAddress}", args.Channel.RemoteAddress);
        };
    }

    public RemoteServer(Guid id, IRemoteConnectionMethod connectionMethod, IClientSocketFactory socketFactory, RemoteInfo? info) : this(id, connectionMethod, socketFactory)
    {
        _remoteInfo.Value = info;
    }
    
    public RemoteServer(Guid id, RemoteDto dto, RemoteCacheDto? cache, IClientSocketFactory socketFactory) : this(id, dto.ConnectionMethod, socketFactory, cache?.RemoteInfo) {}
    
    void IRemoteServerMutationProxy.SetSoftwareInfo(ServerSoftwareInfo info)
    {
        Socket.Channel!.GetAttribute(ServerAttributes.SoftwareInfoAttribute).Set(info);
        SoftwareInfo = info;
    }

    void IRemoteServerMutationProxy.SetRemoteInfo(RemoteInfo info)
    {
        Socket.Channel!.GetAttribute(ServerAttributes.RemoteInfoAttribute).Set(info);
        _remoteInfo.Value = info;
    }

    void IRemoteServerMutationProxy.SetConnectionStatus(IConnectionStatusContext context)
    {
        _connectionStatus.Value = context;
    }
}