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
using Shiki.FileSystem.Implementations.Assembly;
using Shiki.FileSystem.Storage;
using Shiki.FileSystem.Storage.Directory.KeyValue;
using Shiki.FileSystem.Storage.Serializer;
using Shiki.Internationalization;
using Shiki.Internationalization.Component;
using Shiki.Internationalization.Component.Resolver;
using Shiki.Internationalization.Translation;
using Shiki.Internationalization.Translation.LanguageSource;
using Shiki.ModuleManagement;
using UniScan.Core.State;
using UniScan.Device.Device;
using UniScan.Network;
using UniScan.Network.Data.Info.Remote;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Registry;
using UniScan.Network.Registry.Source.Sources;
using UniScan.Network.Server;
using UniScan.Network.Server.Group;
using UniScan.Network.Socket;
using UniScan.Network.Socket.Configuration;
using UniScan.Server.Authentication.Session;
using UniScan.Server.Core.Host;
using UniScan.Server.Core.Module;
using UniScan.Server.Core.UserInterface;
using Constants = UniScan.Core.Constants;

namespace UniScan.Server.Core;

public class UniScanServer
{
    public readonly DeviceHostManager DeviceManager;

    //modules
    private readonly ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs> _moduleStorage;

    private readonly IServiceProvider _serviceProvider;

    //management
    public readonly SessionManager SessionManager;

    public delegate ISocket ServerSocketInitializer(UniScanServerChannelInitializer channelInitializer);

    //networking
    public readonly ISocket Socket;
    private readonly UniScanServerChannelInitializer _channelInitializer;
    private readonly PacketRegistry _packetRegistry;

    public static readonly Identifier Identifier = Constants.IdentifierNamespace.Derived("server");

    public static readonly ServerSoftwareInfo SoftwareInfo = new(
                                                                 new SoftwareAssemblyInfo(Identifier,
                                                                     SemVersion.Parse(Assembly.GetCallingAssembly().InformationalVersionString!)),
                                                                 Network.Constants.ProtocolVersion,
                                                                 "UniScan Server",
                                                                 "https://github.com/UniScanProject/UniScan"
                                                                );

    public static readonly RemoteInfo RemoteInfo = new(
                                                       "UniScan Test Server",
                                                       "Soon I will make all of this configurable",
                                                       new RemoteSettings(true),
                                                       new
                                                           RemoteBranding(new
                                                                              Uri("https://github.com/UniScanProject.png?size=64"),
                                                                          [
                                                                              new
                                                                                  RemoteLink(new
                                                                                          Uri("https://github.com/UniScanProject.png?size=32"),
                                                                                      "UniScan on GitHub",
                                                                                      new
                                                                                          Uri("https://github.com/UniScanProject/UniScan")),
                                                                              new
                                                                                  RemoteLink(new
                                                                                          Uri("https://uniscan.dexrn.me/assets/branding/desktop/pixel/logo.512x512.png"),
                                                                                      "UniScan Web",
                                                                                      new
                                                                                          Uri("https://uniscan.dexrn.me")),
                                                                              new
                                                                                  RemoteLink(new
                                                                                          Uri("https://dexrn.me/favicon.png"),
                                                                                      "Developer's Website",
                                                                                      new Uri("https://dexrn.me"))
                                                                          ]),
                                                       new RemoteSocial("Hello, world!",
                                                                        new Dictionary<Slug<SnakeSlugFormatter>,
                                                                            RemoteAnnouncement>
                                                                        {
                                                                            [new Slug<
                                                                                 SnakeSlugFormatter>("work_in_progress")] =
                                                                                new(
                                                                                 "Work In Progress",
                                                                                 "All of this is still a work in progress, in the future, this will all be configurable.",
                                                                                 DateTimeOffset.UtcNow,
                                                                                 [],
                                                                                 [
                                                                                     new RemoteAnnouncementAuthor(
                                                                                          "Dexrn ZacAttack",
                                                                                          new
                                                                                              Uri("https://github.com/DexrnZacAttack.png?size=64")
                                                                                         )
                                                                                 ])
                                                                        })
                                                      );

    //TODO use TaskPipeline
    public UniScanServer(SessionManager sessionManager, ServerSocketInitializer socketInitializer,
                         PacketRegistry packetRegistry,
                         ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs> moduleStorage)
    {
        SessionManager = sessionManager;
        _moduleStorage = moduleStorage;
        _packetRegistry = packetRegistry;

        DeviceManager = new DeviceHostManager();

        //init di
        ServiceCollection services = new();
        
        services.AddSingleton(_moduleStorage);

        services.AddSingleton(DeviceManager);
        services.AddSingleton(SessionManager);
        
        services.AddSingleton(_packetRegistry);
        
        TranslationManager mgr = new();
        mgr.ApplyLanguagesFromSourceAsync(new DirectoryKeyValueLanguageSource(
                                                     new ReadOnlyDirectoryKeyValueStorage<LanguageFileDto>("/Assets/L10N", new EmbeddedDirectoryManager(Assembly.GetExecutingAssembly()), new EmbeddedFileManager(Assembly.GetExecutingAssembly()), new JsonStorageSerializer<LanguageFileDto>())
                                                    ), "server".ToSlug<SnakeSlugFormatter>(), 20).GetAwaiter().GetResult(); 
        mgr.ApplyLanguagesFromSourceAsync(new DirectoryKeyValueLanguageSource(
                                                     new ReadOnlyDirectoryKeyValueStorage<LanguageFileDto>("/Assets/L10N", new EmbeddedDirectoryManager(typeof(IDeviceState).Assembly), new EmbeddedFileManager(typeof(IDeviceState).Assembly), new JsonStorageSerializer<LanguageFileDto>())
                                                    ), "core".ToSlug<SnakeSlugFormatter>(), 20).GetAwaiter().GetResult();
        
        services.AddSingleton<TranslationManager>(mgr);
        
        foreach (var module in _moduleStorage.Modules) module.Value.ConfigureDi(services);

        services.AddSingleton<ITextComponentResolver, FallbackTextComponentResolver>();
        services.AddSingleton<ITextComponentResolver, AbsoluteTextComponentResolver>();
        services.AddSingleton<ITextComponentResolver, TranslatableTextComponentResolver>();
        
        services.AddSingleton<TextComponentResolverService>();
        
        services.AddSingleton<IUISSRManager, UISSRManager>();
        services.AddSingleton<IUISSRManagerMutationProxy>(p => p.GetRequiredService<IUISSRManager>());
        
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
        }
        catch (TaskCanceledException)
        {
        }
    }

    public async Task ExitAsync()
    {
        Log.Information("Closing socket...");
        if (Socket is ServerSocket serverSocket)
        {
            await serverSocket.ClientManager.BroadcastAsync(new DisconnectPacket(new TranslatableTextComponent("UniScan:i18n/disconnect/server_shutting_down")));
        }

        await Socket.StopAsync();

        Log.Information("Disconnecting devices...");
        await DeviceManager.DisconnectAllAsync();
    }
}