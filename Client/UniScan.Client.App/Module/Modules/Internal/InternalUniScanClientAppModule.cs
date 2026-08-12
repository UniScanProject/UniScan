using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Identity;

namespace UniScan.Client.App.Module.Modules.Internal;

public class InternalUniScanClientAppModule : IUniScanClientAppModule
{
    /// <inheritdoc/>
    public Identifier Id => UniScanApp.Identifier.Derived("modules", "internal");
    
    public void OnInitialize(UniScanClientAppModuleInitializationArgs args)
    {
    }

    public void ConfigureDi(IServiceCollection services)
    {
                
    }
}