using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Identity;
using UniScan.Client.App.Core.Module.Modules.Internal.Handler;
using UniScan.Client.App.Core.Module.Modules.Internal.ViewModelFactory;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.App.Core.Module.Modules.Internal;

public class InternalUniScanClientAppModule : IUniScanClientAppModule
{
    /// <inheritdoc/>
    public Identifier Id => UniScanApp.Identifier.Derived("modules", "internal");
    
    public void OnInitialize(UniScanClientAppModuleInitializationArgs args)
    {
    }

    public void ConfigureDi(IServiceCollection services)
    {
        services.AddSingleton<IUINodeViewModelConverter, TextBlockUIControlViewModelConverter>();
        services.AddSingleton<IUINodeViewModelConverter, ContainerUIControlViewModelConverter>();
        
        services.AddTransient<SetUISlotPacketHandler>();
        
        services.AddSingleton<IPipelineConfigurator, InternalUniScanClientAppPipelineConfigurator>();
    }
}