using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Identity;
using Shiki.Common.Util;
using UniScan.Network.Socket.Configuration;
using UniScan.Server.Core.Module.Modules.Internal.Filter;
using UniScan.Server.Core.Module.Modules.Internal.Handler;

namespace UniScan.Server.Core.Module.Modules.Internal;

public class InternalUniScanServerModule : IUniScanServerModule
{
    /// <inheritdoc/>
    public Identifier Id => Identifier.WithNamespaceOfType<UniScanServer>("modules", "internal");

    private UniScanServer _server = null!;

    public void OnInitialize(UniScanServerModuleInitializationArgs args)
    {
        _server = args.ServerInstance;
    }

    public void ConfigureDi(IServiceCollection services)
    {
        services.AddTransient<SubscribePacketHandler>();
        services.AddTransient<GetDeviceListPacketHandler>();
        services.AddTransient<ClientSoftwareInfoPacketHandler>();
        services.AddTransient<DisconnectPacketHandler>();

        services.AddSingleton(new AcceptedClientPacketFilter());
        
        services.AddSingleton<IPipelineConfigurator, InternalUniScanServerPipelineConfigurator>();
    }
}