using System.Diagnostics.CodeAnalysis;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Identity;
using UniScan.Client.App;
using UniScan.Client.App.Core.Module;
using UniScan.Client.App.UI.ServersideRendering.Binding;
using UniScan.Client.App.Views;
using UniScan.Modules.DebugModule.Views.Debug;

namespace UniScan.Modules.DebugModule;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class UniScanAppDebugModule : IUniScanClientAppModule, IDisposable
{
    public Identifier Id { get; } = UniScanApp.Identifier.Derived("modules", "debug");
    
    private DebugFeaturesManager _debugFeaturesManager;
    
    public void OnInitialize(UniScanClientAppModuleInitializationArgs args)
    {
        UniScanApp.InitializationPipelineFinished += UniScanAppOnInitializationPipelineFinished;
    }

    private async Task UniScanAppOnInitializationPipelineFinished(UniScanApp arg)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            MainViewModel mvm = UniScanApp.ServiceProvider.GetRequiredService<MainViewModel>();

            _debugFeaturesManager =
                new DebugFeaturesManager(UniScanApp.ServiceProvider.GetRequiredService<IUIBindingsRegistry>() as
                                             UIBindingsRegistry, mvm);
            _debugFeaturesManager.Initialize();
        });
    }

    public void Dispose()
    {
        _debugFeaturesManager.Dispose();
    }
}