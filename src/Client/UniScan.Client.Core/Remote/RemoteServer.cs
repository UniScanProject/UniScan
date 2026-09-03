using DotNetty.Common.Utilities;
using ObservableCollections;
using R3;
using Serilog;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Client.Core.Config.Remote;
using UniScan.Client.Core.Remote.Device;
using UniScan.Network;
using UniScan.Network.Client;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Data;
using UniScan.Network.Data.Info.Remote;
using UniScan.Network.Data.Info.Software;
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
    
    public static readonly AttributeKey<string> DisconnectReasonAttribute =
        AttributeKey<string>.ValueOf("disconnectReason");
}

public interface IRemoteServerMutationProxy
{
    void SetSoftwareInfo(ServerSoftwareInfo softwareInfo);

    void SetRemoteInfo(RemoteInfo info);
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

    private readonly ReactiveProperty<bool> _connected = new(false);
    public ReadOnlyReactiveProperty<bool> Connected => _connected;
    
    public RemoteServer(Guid id, IRemoteConnectionMethod connectionMethod, IClientSocketFactory socketFactory)
    {
        Id = id;
        
        ConnectionMethod = connectionMethod;
        DisplayName = _remoteInfo.Select(info => info?.DisplayName ?? ConnectionMethod.ToDisplayString())
                                 .ToReadOnlyBindableReactiveProperty(ConnectionMethod.ToDisplayString());


        Socket = socketFactory.CreateInstance(connectionMethod);
        Socket.ConnectionState.Connected += (sender, args) =>
        {
            _connected.Value = true;
            Log.Information("Connected to {RemoteAddress} over {ConnectionMethod}", args.Channel.RemoteAddress, ConnectionMethod);
        };
        
        Socket.ConnectionState.Disconnected += (sender, args) =>
        {
            _connected.Value = false;
            
            Devices.Clear();
            
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
}