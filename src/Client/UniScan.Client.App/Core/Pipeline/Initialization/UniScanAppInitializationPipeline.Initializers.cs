using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views;
using UniScan.Client.App.Views.Settings;
using UniScan.Client.Core.Remote;
using UniScan.Network;
using UniScan.Network.Registry;
using UniScan.Network.Registry.Source.Sources;
using MainViewModel = UniScan.Client.App.Views.MainViewModel;

namespace UniScan.Client.App.Core.Pipeline.Initialization;

public partial class UniScanAppInitializationPipeline
{
    internal static async Task SetupSSRSlotRegistry(TaskContexts.Early ctx, CancellationToken ct = default)
    {
        ctx.ServiceCollection.AddSingleton<IUISlotRegistry, UISlotRegistry>();
    }
    
    internal static async Task SetupSSRUI(TaskContexts.Early ctx, CancellationToken ct = default)
    {
        //modules have been loaded by this point and DI has been initialized, now we set up our ssr ui factories
        ctx.ServiceCollection.AddSingleton<IUIViewFactory, UIViewFactory>();
    }
    
    internal static async Task InitializeViews(TaskContexts.PostClient ctx, CancellationToken ct = default)
    {
        ctx.Status.Value = "Initializing UI";
        
        ctx.ServiceCollection.AddSingleton<MainView>();
        ctx.ServiceCollection.AddSingleton<MainViewModel>();

        ctx.ServiceCollection.AddSingleton<ClientSettingsViewModel>();
    }
    
    internal static async Task InitializeServiceProvider(TaskContexts.PreServiceProvider ctx, CancellationToken ct = default)
    {
        ctx.Status.Value = "Building ServiceProvider";

        ctx.Services = ctx.ServiceCollection.BuildServiceProvider();
    }

    internal static async Task InitializeRemotes(TaskContexts.PostServiceProvider ctx, CancellationToken ct = default)
    {
        ctx.Status.Value = "Loading remotes";
        
        IRemoteStorage remoteStorage = ctx.Services.GetRequiredService<IRemoteStorage>();
        await remoteStorage.LoadAsync();
        
        IRemoteManager remoteManager = ctx.Services.GetRequiredService<IRemoteManager>();
        Log.Information("Loaded {Count} remote(s)", remoteManager.Remotes.Count);
    }
    
    internal Task RegisterPackets(UniScanAppInitializationPipeline.TaskContexts.PostServiceProvider ctx,
                                  CancellationToken ct = default)
    {
        ctx.Status.Value = "Registering packets";
        
        PacketRegistry registry = ctx.Services.GetRequiredService<PacketRegistry>();
        registry.RegisterFromSource<AssembliesPacketSource>();

        return Task.CompletedTask;
    }
}