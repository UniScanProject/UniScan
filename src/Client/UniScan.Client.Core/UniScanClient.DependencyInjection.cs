using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using UniScan.Client.Core.Config.Remote;
using UniScan.Client.Core.DI.Factory;
using UniScan.Client.Core.Module;
using UniScan.Client.Core.Module.Modules.Internal;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Storage;
using UniScan.Client.Core.Storage.Serializer;
using UniScan.Core.Serialization;
using UniScan.Network;
using UniScan.Network.Client;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Registry;
using UniScan.Platform;

namespace UniScan.Client.Core;

public static class UniScanClientDependencyInjection
{
    public static IServiceCollection AddUniScanClient(this IServiceCollection services, HostEnvironment environment,
                                                      ClientSoftwareInfo softwareInfo)
    {
        services.AddSingleton(softwareInfo);
        
        var moduleStorage = new ModuleStorage<IUniScanClientModule, UniScanClientModuleInitializationArgs>()
                           .WithModulesFrom(new TypeListModuleSource(typeof(InternalUniScanClientModule)), new UniScanClientModuleInitializationArgs())
                           .WithModulesFrom(new AssembliesModuleSource(Path.Combine(environment.StandardPaths.DataPath, "modules")), new UniScanClientModuleInitializationArgs());
        foreach (IUniScanClientModule module in moduleStorage.Modules)
        {
            module.ConfigureDi(services);
        }
        
        services.AddKeyedSingleton<JsonSerializerOptions>("PolymorphicJsonOptions", (_, _) => PolymorphicJsonOptionsFactory.Get());
        
        services.AddSingleton<PacketRegistry>();
        services.AddSingleton<IClientSocketFactory, ClientSocketFactory>();
        services.AddSingleton<IRemoteManager, RemoteManager>();
        services.AddSingleton<IRemoteFactory, RemoteFactory>();

        services.AddSingleton<DirectoryKeyValueStorage<RemoteDto>>(provider =>
                                                                       new DirectoryKeyValueStorage<RemoteDto>(
                                                                            Path.Combine(environment.StandardPaths.ConfigPath, "remotes"),
                                                                            environment.DirectoryManager,
                                                                            environment.FileManager,
                                                                            new JsonStorageSerializer<RemoteDto>(provider.GetRequiredKeyedService<JsonSerializerOptions>("PolymorphicJsonOptions")),
                                                                            provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DirectoryKeyValueStorage<RemoteDto>>>()
                                                                           ));
        
        services.AddSingleton<DirectoryKeyValueStorage<RemoteCacheDto>>(provider =>
                                                                       new DirectoryKeyValueStorage<RemoteCacheDto>(
                                                                            Path.Combine(environment.StandardPaths.CachePath, "remotes"),
                                                                            environment.DirectoryManager,
                                                                            environment.FileManager,
                                                                            new JsonStorageSerializer<RemoteCacheDto>(provider.GetRequiredKeyedService<JsonSerializerOptions>("PolymorphicJsonOptions")),
                                                                            provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DirectoryKeyValueStorage<RemoteCacheDto>>>()
                                                                           ));
        
        services.AddSingleton<IRemoteStorage, RemoteStorage>();
        
        services.AddSingleton<UniScanClient>();

        return services;
    }
}