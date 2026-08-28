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
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Util;
using Shiki.ModuleManagement;
using UniScan.Device.Device;
using UniScan.Network;
using UniScan.Network.Data.Info.Remote;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Packet.Packets.Bidirectional.Status;
using UniScan.Network.Packet.Packets.Clientbound.Remote;
using UniScan.Network.Registry.Source.Sources;
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
                                                                 SemVersion.Parse(Assembly.GetCallingAssembly().InformationalVersionString),
                                                                 Network.Constants.ProtocolVersion,
                                                                 "UniScan Server",
                                                                 "https://github.com/UniScanProject/UniScan"
                                                                 );
    
    public static readonly RemoteInfo RemoteInfo = new(
                                                                 "UniScan Test Server",
                                                                 "Soon I will make all of this configurable",
                                                                 new RemoteSettings(true),
                                                                 new RemoteBranding(new Uri("https://github.com/UniScanProject.png?size=64"), [
                                                                    new RemoteLink(new Uri("https://github.com/UniScanProject.png?size=32"), "UniScan on GitHub", new Uri("https://github.com/UniScanProject/UniScan")),
                                                                    new RemoteLink(new Uri("https://uniscan.dexrn.me/assets/logo_512x512.png"), "UniScan Web", new Uri("https://uniscan.dexrn.me")),
                                                                    new RemoteLink(new Uri("https://dexrn.me/favicon.png"), "Developer's Website", new Uri("https://dexrn.me"))
                                                                 ]),
                                                                 new RemoteSocial("Hello, world!", new Dictionary<Slug<SnakeSlugFormatter>, RemoteAnnouncement> {
                                                                    [new Slug<SnakeSlugFormatter>("work_in_progress")] = new(
                                                                     "Work In Progress",
                                                                     "All of this is still a work in progress, in the future, this will all be configurable.",
                                                                     DateTimeOffset.UtcNow,
                                                                     [],
                                                                     [
                                                                         new RemoteAnnouncementAuthor(
                                                                          "Dexrn ZacAttack",
                                                                          new Uri("https://github.com/DexrnZacAttack.png?size=64")
                                                                          )
                                                                     ])
                                                                 })
                                                                );
    
    public UniScanServer(SessionManager sessionManager, ServerSocketInitializer socketInitializer, PacketRegistry packetRegistry, ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs> moduleStorage)
    {
        SessionManager = sessionManager;
        _moduleStorage = moduleStorage;
        _packetRegistry = packetRegistry;
        _networkGroupManager = new MultithreadedGroupManager();
        
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
        _packetRegistry.RegisterFromSource<AssembliesPacketSource>();
        
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
        Log.Information("Closing socket...");
        if (Socket is ServerSocket serverSocket)
        {
            await serverSocket.ClientManager.BroadcastAsync(new DisconnectPacket("Server shutting down..."));
        }

        await Socket.StopAsync();
        
        Log.Information("Disconnecting scanners...");
        await ScannerManager.DisconnectAllAsync();
    }
}