using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Semver;
using Shiki.Common.Extensions;
using Shiki.Common.Identity;
using Shiki.Common.Util;
using UniScan.Network.Socket.Configuration;
using UniScan.Server.Core.Module.Modules.Internal.Filter;
using UniScan.Server.Core.Module.Modules.Internal.Handler;
using UniScan.Server.Core.UserInterface;
using UniScan.Server.Core.UserInterface.Providers;

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
        services.AddTransient<IUISSRContextSessionFactory, DeviceSSRContextSessionFactory>();
        
        services.AddSingleton<IUINodeProvider, DefaultDeviceNodeProvider>();
        services.AddSingleton<IUINodeProvider, DefaultScanChannelNodeProvider>();
        services.AddSingleton<IUINodeProvider, DefaultScanGroupNodeProvider>();
        services.AddSingleton<IUINodeProvider, DefaultScanListNodeProvider>();
        services.AddSingleton<IUINodeProvider, DefaultScanZoneNodeProvider>();
        
        services.AddTransient<UIContextUnsubscribePacketHandler>();
        services.AddTransient<UIContextSubscribePacketHandler>();
        services.AddTransient<SubscribePacketHandler>();
        services.AddTransient<GetDeviceListPacketHandler>();
        services.AddTransient<ClientSoftwareInfoPacketHandler>();
        services.AddTransient<DisconnectPacketHandler>();

        services.AddSingleton(new AcceptedClientPacketFilter());
        
        services.AddSingleton<IPipelineConfigurator, InternalUniScanServerPipelineConfigurator>();
    }

    public void Dispose()
    {
    }
}