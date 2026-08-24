using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using R3;
using Serilog;
using Shiki.Common.Collections;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Client.Core.Config.Remote;
using UniScan.Core.State;
using UniScan.Network;
using UniScan.Network.Client;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Data;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Packet.Packets.Clientbound;
using UniScan.Network.Packet.Packets.Clientbound.Remote;
using UniScan.Network.Packet.Packets.Serverbound;
using UniScan.Network.Packet.Packets.Serverbound.Client;
using UniScan.Network.Request;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.Core.Config.Types;

public class ServerAttributes
{
    public static readonly AttributeKey<ServerSoftwareInfo> SoftwareInfoAttribute =
        AttributeKey<ServerSoftwareInfo>.ValueOf("software");
    
    public static readonly AttributeKey<RemoteServer> ServerAttribute =
        AttributeKey<RemoteServer>.ValueOf("server");
    
    public static readonly AttributeKey<string> DisconnectReasonAttribute =
        AttributeKey<string>.ValueOf("disconnectReason");
}

public interface IRemoteServerMutationProxy
{
    void SetSoftwareInfo(ServerSoftwareInfo softwareInfo);
}

public class RemoteServer : IRemoteServerMutationProxy
{
    public string DisplayName => ConnectionMethod.ToDisplayString();//todo cache
    
    /// <summary>
    /// connection method
    /// </summary>
    public IRemoteConnectionMethod ConnectionMethod { get; }
    
    /// <summary>
    /// cached list of devices
    /// </summary>
    public ObservableDictionary<Slug<SnakeSlugFormatter>, DeviceDto> Devices { get; } = [];
    
    //todo Make it cached
    public ServerSoftwareInfo? SoftwareInfo { get; private set; }
    
    public ClientSocket Socket { get; }

    private readonly ReactiveProperty<bool> _connected = new(false);
    public ReadOnlyReactiveProperty<bool> Connected => _connected;
    
    private readonly IServiceProvider _serviceProvider;

    public RemoteServer(IRemoteConnectionMethod connectionMethod, IEnumerable<IPipelineConfigurator> configurators, PacketRegistry packetRegistry, IServiceProvider serviceProvider)
    {
        ConnectionMethod = connectionMethod;
        _serviceProvider = serviceProvider;
        
        Socket = new ClientSocket(new UniScanClientChannelInitializer(packetRegistry, configurators, _serviceProvider), ConnectionMethod);
        Socket.ConnectionState.Connected += (sender, args) =>
        {
            _connected.Value = true;
            Log.Information("Connected to {RemoteAddress} over {ConnectionMethod}", args.Channel.RemoteAddress, ConnectionMethod);
        };
        
        Socket.ConnectionState.Disconnected += (sender, args) =>
        {
            _connected.Value = false;
            Log.Information("Disconnected from {RemoteAddress}", args.Channel.RemoteAddress);
        };
    }
    
    public RemoteServer(RemoteDto dto, IEnumerable<IPipelineConfigurator> configurators, PacketRegistry packetRegistry, IServiceProvider serviceProvider) : this(dto.ConnectionMethod, configurators, packetRegistry, serviceProvider) {}

    void IRemoteServerMutationProxy.SetSoftwareInfo(ServerSoftwareInfo info)
    {
        Socket.Channel!.GetAttribute(ServerAttributes.SoftwareInfoAttribute).Set(info);
        SoftwareInfo = info;
    }
}