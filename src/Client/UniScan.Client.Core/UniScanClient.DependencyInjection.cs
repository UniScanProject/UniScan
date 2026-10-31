using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shiki.FileSystem.Abstractions;
using Shiki.FileSystem.Storage;
using Shiki.FileSystem.Storage.Directory.KeyValue;
using Shiki.FileSystem.Storage.Serializer;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using UniScan.Client.Core.Config.Remote;
using UniScan.Client.Core.DI.Factory;
using UniScan.Client.Core.Module;
using UniScan.Client.Core.Module.Modules.Internal;
using UniScan.Client.Core.Remote;
using UniScan.Core.Serialization;
using UniScan.Network;
using UniScan.Network.Client;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Registry;
using UniScan.Platform;

namespace UniScan.Client.Core;

public static class 
    UniScanClientDependencyInjection
{
    public static async Task<IServiceCollection> AddUniScanClientAsync(this IServiceCollection services, IPlatformStandardPaths paths, IPlatformFileManager fileManager, IPlatformDirectoryManager directoryManager,
                                                      ClientSoftwareInfo softwareInfo)
    {
        ILogger logger = Log.ForContext("SourceContext", nameof(UniScanClientDependencyInjection));
        
        services.AddSingleton(softwareInfo);
        
        var moduleStorage = await new ModuleStorageBuilder<IUniScanClientModule, UniScanClientModuleInitializationArgs>()
                           .WithModuleOfType<InternalUniScanClientModule>(null)
                           .WithSource(new AssembliesModuleSource(Path.Combine(paths.DataPath, "modules")), new UniScanClientModuleInitializationArgs())
                           .BuildAsync();
        
        services.AddSingleton(moduleStorage);
        
        foreach (var module in moduleStorage.Modules)
        {
            module.Value.ConfigureDi(services);
            services.AddSingleton<IUniScanClientModule>(module.Value);
            
            logger.Information("Initialized client module '{Id}' ({TypeName})", module.Key, module.Value.Id);
        }
        
        services.AddKeyedSingleton<JsonSerializerOptions>("PolymorphicJsonOptions", (_, _) => new JsonSerializerOptions()
        {
            TypeInfoResolver = PolymorphicJsonTypeInfoResolverFactory.Get(new DefaultJsonTypeInfoResolver())
        });
        
        services.AddSingleton<PacketRegistry>();
        services.AddSingleton<IClientSocketFactory, ClientSocketFactory>();
        services.AddSingleton<IRemoteManager, RemoteManager>();
        services.AddSingleton<IRemoteFactory, RemoteFactory>();

        services.AddSingleton<DirectoryKeyValueStorage<RemoteDto>>(provider =>
                                                                       new DirectoryKeyValueStorage<RemoteDto>(
                                                                            Path.Combine(paths.ConfigPath, "remotes"),
                                                                            directoryManager,
                                                                            fileManager,
                                                                            new JsonStorageSerializer<RemoteDto>(provider.GetRequiredKeyedService<JsonSerializerOptions>("PolymorphicJsonOptions"))
                                                                           ));
        
        services.AddSingleton<DirectoryKeyValueStorage<CachedRemoteInfoDto>>(provider =>
                                                                       new DirectoryKeyValueStorage<CachedRemoteInfoDto>(
                                                                            Path.Combine(paths.CachePath, "remotes"),
                                                                            directoryManager,
                                                                            fileManager,
                                                                            new JsonStorageSerializer<CachedRemoteInfoDto>(provider.GetRequiredKeyedService<JsonSerializerOptions>("PolymorphicJsonOptions"))
                                                                           ));
        
        services.AddSingleton<IRemoteStorage, RemoteStorage>();
        
        services.AddSingleton<UniScanClient>();

        return services;
    }
}