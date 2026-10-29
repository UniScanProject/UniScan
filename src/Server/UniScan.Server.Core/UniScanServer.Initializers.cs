using System.Collections.Immutable;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shiki.TaskPipeline.Context;
using UniScan.Core.Serialization;
using UniScan.Network.Group;
using UniScan.Network.Registry;
using UniScan.Network.Registry.Source.Sources;
using UniScan.Network.Server;
using UniScan.Network.Server.HostMethod;
using UniScan.Network.Socket.Configuration;
using UniScan.Server.Authentication.Session;
using UniScan.Server.Core.Host;
using UniScan.Server.Core.Pipeline;
using UniScan.Server.Core.Social;

namespace UniScan.Server.Core;

public partial class UniScanServer
{
    internal void InitializeServer(UniScanServerInitializationPipeline.TaskContexts.PreEnvironment ctx,
                                         ITaskExecutionContext execCtx)
    {
        execCtx.Status.Value = "Initializing server class";
        
        RootDirectory = ctx.RootDirectory;
        _moduleStorage = ctx.ModuleStorage;

        _packetRegistry = new PacketRegistry();
        SessionManager = new SessionManager();
        DeviceManager = new DeviceHostManager();

        ctx.ServiceCollection.AddSingleton(DeviceManager);
        ctx.ServiceCollection.AddSingleton(SessionManager);
        ctx.ServiceCollection.AddSingleton(_packetRegistry);
        ctx.ServiceCollection.AddSingleton(ctx.ModuleStorage);
    }

    internal void ConfigureModules(UniScanServerInitializationPipeline.TaskContexts.PreServiceProvider ctx,
                                          ITaskExecutionContext execCtx)
    {
        foreach (var module in _moduleStorage.Modules) module.Value.ConfigureDi(ctx.ServiceCollection);
    }

    internal async Task InitializeSettingsAsync(UniScanServerInitializationPipeline.TaskContexts.PreSettings ctx,
                                           ITaskExecutionContext execCtx)
    {
        execCtx.Status.Value = "Loading settings";
        
        var res = await ctx.SettingsBuilder.BuildAndLoadAsync();
        
        var errors = res.Results.Where(r => r is { IsSuccessful: false, Error: not null }).Select(e => e.Error!).ToArray();
        if (errors.Length != 0)
        {

            StringBuilder b = new();
            foreach (Exception r in errors)
            {
                b.Append($"- {r.ToString()}\n");
            }
            
            Log.Error("Errors were encountered during settings load: \n{Errors}", b);
            
            throw new AggregateException(errors);//halt server because if settings are fucked this could cause administration issues
        }

        ctx.SettingsManager = res.settingsManager;
        ctx.ServiceCollection.AddSingleton(_ => res.settingsManager);
        
        Settings = ctx.SettingsManager;
    }
    
    internal void InitializeRemoteInfoManager(UniScanServerInitializationPipeline.TaskContexts.PreServiceProvider ctx,
                                   ITaskExecutionContext execCtx)
    {
        execCtx.Status.Value = "Initializing remote info manager";
        
        ctx.ServiceCollection.AddSingleton(sp => new RemoteInfoManager(Path.Combine(ctx.RootDirectory, "settings"), ctx.FileManager,
                                                                       ctx.DirectoryManager,
                                                                       PolymorphicJsonTypeInfoResolverFactory.FancySerializerOptions));
    }

    internal UniScanServerInitializationPipeline.TaskContexts.ServerReady InitializeServiceProvider(UniScanServerInitializationPipeline.TaskContexts.PreServiceProvider ctx,
                                                                                                         ITaskExecutionContext execCtx)
    {
        execCtx.Status.Value = "Initializing Serverside Rendering";

        ServiceProvider provider = ctx.ServiceCollection.BuildServiceProvider();
        ServiceProvider = provider;

        return new UniScanServerInitializationPipeline.TaskContexts.ServerReady(this, provider);
    }
    
    public void InitializeNetworking(UniScanServerInitializationPipeline.TaskContexts.ServerReady ctx,
                                            ITaskExecutionContext execCtx)
    {
        execCtx.Status.Value = "Initializing Networking";
        
        _packetRegistry.RegisterFromSource<AssembliesPacketSource>();
            
        //now init channel initializer
        var configurators = ctx.ServiceProvider.GetServices<IPipelineConfigurator>().ToImmutableList();
        _channelInitializer = new UniScanServerChannelInitializer(_packetRegistry, configurators, ctx.ServiceProvider);

        //init socket
        Socket = new ServerSocket(_channelInitializer, Settings.TryGetGroup<MainOptionGroup>()?.HostMethods ?? [], ctx.ServiceProvider.GetRequiredService<IHostNetworkGroupManager>());
    }

    public void FinishInitialization(UniScanServerInitializationPipeline.TaskContexts.ServerReady ctx,
                                     ITaskExecutionContext execCtx)
    {
        execCtx.Status.Value = "Finishing up";
        
        RemoteInfoManager = ctx.ServiceProvider.GetRequiredService<RemoteInfoManager>();
    }
}