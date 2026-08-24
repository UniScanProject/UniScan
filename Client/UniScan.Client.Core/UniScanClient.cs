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
using UniScan.Client.Core.Config.Types;
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
    : IAsyncFactoryConstructable<UniScanClient, HostEnvironment, ClientSoftwareInfo>, IDisposable
{
    public IRemoteManager RemoteManager { get; } = remoteManager;
    public IFile<IRemoteManager> RemoteManagerFile { get; } = remotesListFile;

    private ModuleStorage<IUniScanClientModule, UniScanClientModuleInitializationArgs> _moduleStorage = moduleStorage;

    public IPlatformStandardPaths Paths { get; } = paths;

    public JsonSerializerOptions JsonSerializerOptions { get; } = jsonSerializerOptions;

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public static readonly Identifier ClientIdentifier = Constants.IdentifierNamespace.Derived("client");

    private readonly CompositeDisposable _disposables = new();
    
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
        JsonSerializerOptions opt = serviceProvider.GetRequiredKeyedService<JsonSerializerOptions>("PolymorphicJsonOptions");
        
        logger.Information("Loading remotes");
        RemotesListFile remotesListFile = new(environment.StandardPaths.ConfigPath, environment.FileManager, rf, opt);
        IRemoteManager remoteManager = await remotesListFile.LoadAsync();
        
        logger.Information("Loaded {Amount} remotes", remoteManager.Remotes.Count);
        
        logger.Information("Constructing client");
        UniScanClient c = new(environment.StandardPaths, moduleStorage, remotesListFile, remoteManager, opt, serviceProvider);
        
        logger.Information("Adding remote listeners");
        c.SetupObserveRemotes();

        return c;
    }

    private void SetupObserveRemotes()
    {
        RemoteManager.Remotes.ObserveAdd().Subscribe(async _ => await OnRemoteModified()).AddTo(_disposables);
        RemoteManager.Remotes.ObserveRemove().Subscribe(async _ => await OnRemoteModified()).AddTo(_disposables);

        foreach (RemoteServer remote in RemoteManager.Remotes)
        {
            ObserveRemote(remote);
        }
        
        RemoteManager.Remotes.ObserveAdd().Subscribe(remote => ObserveRemote(remote.Value)).AddTo(_disposables);
    }

    private void ObserveRemote(RemoteServer remote)
    {
        remote.RemoteInfo.Skip(1).Subscribe(async _ => await OnRemoteModified()).AddTo(_disposables);
    }
    
    private async Task OnRemoteModified()
    {
        await RemoteManagerFile.SaveAsync(RemoteManager);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}