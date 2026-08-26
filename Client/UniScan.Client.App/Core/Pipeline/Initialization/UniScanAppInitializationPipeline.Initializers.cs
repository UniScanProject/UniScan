using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UniScan.Client.App.ViewModels;
using UniScan.Client.App.Views;
using UniScan.Client.Core.Remote;
using UniScan.Network;
using UniScan.Network.Registry.Source.Sources;

namespace UniScan.Client.App.Core.Pipeline.Initialization;

public partial class UniScanAppInitializationPipeline
{
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