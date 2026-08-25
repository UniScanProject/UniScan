using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using R3;
using Serilog;
using Shiki.Common.Identity;
using Shiki.Common.Factory;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using UniScan.Client.Core.Config;
using UniScan.Client.Core.Config.Remote;
using UniScan.Client.Core.DI.Factory;
using UniScan.Client.Core.Module;
using UniScan.Client.Core.Module.Modules.Internal;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Storage;
using UniScan.Client.Core.Storage.Serializer;
using UniScan.Core.Serialization;
using UniScan.Network;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Registry.Source.Sources;
using UniScan.Network.Socket.Configuration;
using UniScan.Platform;
using Constants = UniScan.Core.Constants;

namespace UniScan.Client.Core;

public partial class UniScanClient(
    IPlatformStandardPaths paths,
    ModuleStorage<IUniScanClientModule, UniScanClientModuleInitializationArgs> moduleStorage,
    IRemoteManager remoteManager,
    JsonSerializerOptions jsonSerializerOptions,
    IServiceProvider serviceProvider)
    : IAsyncFactoryConstructable<UniScanClient, HostEnvironment, ClientSoftwareInfo>
{
    public IRemoteManager RemoteManager { get; } = remoteManager;

    private ModuleStorage<IUniScanClientModule, UniScanClientModuleInitializationArgs> _moduleStorage = moduleStorage;

    public IPlatformStandardPaths Paths { get; } = paths;

    public JsonSerializerOptions JsonSerializerOptions { get; } = jsonSerializerOptions;

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public static readonly Identifier ClientIdentifier = Constants.IdentifierNamespace.Derived("client");


    public static async Task<UniScanClient> CreateInstanceAsync(HostEnvironment environment,
                                                                ClientSoftwareInfo softwareInfo)
    {
        ILogger logger = Log.Logger.ForContext<UniScanClient>();

        var moduleStorage = new ModuleStorage<IUniScanClientModule, UniScanClientModuleInitializationArgs>()
                           .WithModulesFrom(new TypeListModuleSource(typeof(InternalUniScanClientModule)),
                                            new UniScanClientModuleInitializationArgs())
                           .WithModulesFrom(new AssembliesModuleSource(Path.Combine(environment.StandardPaths.DataPath,
                                                                           "modules")),
                                            new UniScanClientModuleInitializationArgs());

        logger.Information("Loaded {Amount} modules", moduleStorage.Modules.Count);

        ServiceCollection services = new();
        services.AddSingleton(softwareInfo);

        services.AddLogging(logging => logging.AddSerilog(Log.Logger));

        foreach (IUniScanClientModule module in moduleStorage.Modules)
        {
            module.ConfigureDi(services);
        }

        services.AddKeyedSingleton("PolymorphicJsonOptions", PolymorphicJsonOptionsFactory.Get());
        services.AddSingleton<IRemoteFactory, RemoteFactory>();

        services.AddSingleton<PacketRegistry>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        logger.Information("Registering packets");
        PacketRegistry registry = serviceProvider.GetRequiredService<PacketRegistry>();
        registry.RegisterFromSource<AssembliesPacketSource>();

        IRemoteFactory rf = serviceProvider.GetRequiredService<IRemoteFactory>();
        JsonSerializerOptions opt =
            serviceProvider.GetRequiredKeyedService<JsonSerializerOptions>("PolymorphicJsonOptions");

        logger.Information("Loading remotes");
        RemoteManager remoteManager = new();

        RemoteStorage storage = new(remoteManager, rf, new DirectoryKeyValueStorage<RemoteDto>(
                                         Path.Combine(environment.StandardPaths.ConfigPath, "remotes"),
                                         environment.DirectoryManager,
                                         environment.FileManager,
                                         new JsonStorageSerializer<RemoteDto>(opt),
                                         serviceProvider
                                            .GetRequiredService<Microsoft.Extensions.Logging.ILogger<
                                                 DirectoryKeyValueStorage<RemoteDto>>>()
                                        ),
                                    new DirectoryKeyValueStorage<RemoteCacheDto>(
                                         Path.Combine(environment.StandardPaths.CachePath, "remotes"),
                                         environment.DirectoryManager,
                                         environment.FileManager,
                                         new JsonStorageSerializer<RemoteCacheDto>(opt),
                                         serviceProvider
                                            .GetRequiredService<Microsoft.Extensions.Logging.ILogger<
                                                 DirectoryKeyValueStorage<RemoteCacheDto>>>()
                                        ));

        await storage.LoadAsync();

        logger.Information("Loaded {Amount} remotes", remoteManager.Remotes.Count);

        logger.Information("Constructing client");
        UniScanClient c = new(environment.StandardPaths, moduleStorage, remoteManager, opt,
                              serviceProvider);

        logger.Information("Adding remote listeners");

        return c;
    }
}