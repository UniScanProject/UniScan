using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Microsoft.Extensions.DependencyInjection;
using Semver;
using Serilog;
using Serilog.Core;
using Shiki.Common.Extensions;
using Shiki.Common.Identity;
using Shiki.Common.Util;
using Shiki.ModuleManagement;
using UniScan.Device.Device;
using UniScan.Network;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Packet.Packets.Clientbound.Remote;
using UniScan.Network.Server;
using UniScan.Network.Server.Group;
using UniScan.Network.Socket;
using UniScan.Network.Socket.Configuration;
using UniScan.Server.Authentication.Session;
using UniScan.Server.Core.Host;
using UniScan.Server.Core.Module;
using Constants = UniScan.Core.Constants;

namespace UniScan.Server.Core;

public class UniScanServer
{
    public readonly ScannerHostManager ScannerManager;
    
    //modules
    private readonly ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs> _moduleStorage;

    private readonly IServiceProvider _serviceProvider;
    
    //management
    public readonly SessionManager SessionManager;
    private readonly IGroupManager _networkGroupManager;
    
    public delegate ISocket ServerSocketInitializer(UniScanServerChannelInitializer channelInitializer);
    
    //networking
    public readonly ISocket Socket;
    private readonly UniScanServerChannelInitializer _channelInitializer;
    private readonly PacketRegistry _packetRegistry;

    public static readonly Identifier Identifier = Constants.IdentifierNamespace.Derived("server");
    public static readonly ServerSoftwareInfo SoftwareInfo = new(
                                                                 Identifier,
                                                                 SemVersion.FromVersion(Assembly.GetCallingAssembly().Version),
                                                                 Network.Constants.ProtocolVersion,
                                                                 "UniScan Server",
                                                                 "https://github.com/UniScanProject/UniScan"
                                                                 );
    
    public UniScanServer(SessionManager sessionManager, ServerSocketInitializer socketInitializer, PacketRegistry packetRegistry, ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs> moduleStorage)
    {
        SessionManager = sessionManager;
        _moduleStorage = moduleStorage;
        _packetRegistry = packetRegistry;
        _networkGroupManager = new LibUvGroupManager(); //new MultithreadedGroupManager();
        
        ScannerManager = new ScannerHostManager();
        
        //init di
        ServiceCollection services = new();
        
        services.AddSingleton(ScannerManager);
        services.AddSingleton(SessionManager);
        
        foreach (IUniScanServerModule module in _moduleStorage.Modules)
        {
            module.ConfigureDi(services);
        }
        _serviceProvider = services.BuildServiceProvider();
        
        
        //init packets
        var packetConfigurators = _serviceProvider.GetServices<IPacketConfigurator>();
        foreach (IPacketConfigurator configurator in packetConfigurators)
        {
            configurator.ConfigurePackets(_packetRegistry);
        } 
        
        //now init channel initializer
        var configurators = _serviceProvider.GetServices<IPipelineConfigurator>().ToImmutableList();
        _channelInitializer = new UniScanServerChannelInitializer(_packetRegistry, configurators, _serviceProvider);
        
        //init socket
        Socket = socketInitializer(_channelInitializer);
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        await Socket.StartAsync();

        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        } catch (TaskCanceledException) {}
    }

    public async Task ExitAsync()
    {
        await Socket.StopAsync();
        await ScannerManager.DisconnectAllAsync();
    }
}