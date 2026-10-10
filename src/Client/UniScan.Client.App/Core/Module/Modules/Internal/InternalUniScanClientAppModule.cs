using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Identity;
using UniScan.Client.App.Core.Module.Modules.Internal.ViewConverter;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.Core.Module.Modules.Internal.Handler;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.App.Core.Module.Modules.Internal;

public class InternalUniScanClientAppModule : IUniScanClientAppModule
{
    /// <inheritdoc/>
    public Identifier Id => UniScanApp.Identifier.Derived("modules", "internal");

    public void ConfigureDi(IServiceCollection services)
    {
        services.AddSingleton<IUINodeViewConverter, TextBlockUIControlViewConverter>();
        services.AddSingleton<IUINodeViewConverter, ContainerUIControlViewConverter>();
        
        services.AddSingleton<IPipelineConfigurator, InternalUniScanClientAppPipelineConfigurator>();
    }

    public void Dispose()
    {
        
    }
}