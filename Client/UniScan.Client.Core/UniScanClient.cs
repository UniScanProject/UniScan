using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
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
using UniScan.Core.Serialization;
using UniScan.Network;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Registry.Source.Sources;
using UniScan.Network.Socket.Configuration;
using UniScan.Platform;
using Constants = UniScan.Core.Constants;

namespace UniScan.Client.Core;

public class UniScanClient(
    IPlatformStandardPaths paths,
    ModuleStorage<IUniScanClientModule, UniScanClientModuleInitializationArgs> moduleStorage,
    RemotesListFile remotesListFile,
    IRemoteManager remoteManager,
    JsonSerializerOptions jsonSerializerOptions,
    IServiceProvider serviceProvider)
    : IAsyncFactoryConstructable<UniScanClient, HostEnvironment, ClientSoftwareInfo>
{
    public IRemoteManager RemoteManager { get; } = remoteManager;
    public IFile<IRemoteManager> RemoteManagerFile { get; } = remotesListFile;

    private ModuleStorage<IUniScanClientModule, UniScanClientModuleInitializationArgs> _moduleStorage = moduleStorage;

    public IPlatformStandardPaths Paths { get; } = paths;

    public JsonSerializerOptions JsonSerializerOptions { get; } = jsonSerializerOptions;

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public static readonly Identifier ClientIdentifier = Constants.IdentifierNamespace.Derived("client");
    
    public static async Task<UniScanClient> CreateInstanceAsync(HostEnvironment environment, ClientSoftwareInfo softwareInfo)
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
        JsonSerializerOptions opt = serviceProvider.GetRequiredKeyedService<JsonSerializerOptions>("PolymorphicJsonOptions");
        
        logger.Information("Loading remotes");
        RemotesListFile remotesListFile = new(environment.StandardPaths.ConfigPath, environment.FileManager, rf, opt);
        IRemoteManager remoteManager = await remotesListFile.LoadAsync();
        
        logger.Information("Loaded {Amount} remotes", remoteManager.Remotes.Count);

        return new UniScanClient(environment.StandardPaths, moduleStorage, remotesListFile, remoteManager, opt, serviceProvider);
    }
}