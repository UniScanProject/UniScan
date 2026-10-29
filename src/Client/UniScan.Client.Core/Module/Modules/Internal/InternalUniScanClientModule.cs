using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Identity;
using UniScan.Client.Core.Module.Modules.Internal.Handler;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.Core.Module.Modules.Internal;

public class InternalUniScanClientModule : IUniScanClientModule
{
    /// <inheritdoc/>
    public Identifier Id => UniScanClient.Identifier.Derived("modules", "internal");
    
    public void OnInitialize(UniScanClientModuleInitializationArgs args)
    {
    }

    public void ConfigureDi(IServiceCollection services)
    {
        services.AddTransient<SetUISlotPacketHandler>();
        services.AddTransient<SetUINodePacketHandler>();
        
        services.AddTransient<DisconnectPacketHandler>();
        services.AddTransient<RemoteInfoPacketHandler>();
        services.AddTransient<DeviceStatePacketHandler>();
        
        services.AddSingleton<IPipelineConfigurator, InternalUniScanClientPipelineConfigurator>();
    }

    public void Dispose() { }
}