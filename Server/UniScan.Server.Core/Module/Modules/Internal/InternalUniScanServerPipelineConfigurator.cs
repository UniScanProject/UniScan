using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;
using UniScan.Network.Socket.Configuration;
using UniScan.Server.Core.Module.Modules.Internal.Filter;
using UniScan.Server.Core.Module.Modules.Internal.Handler;

namespace UniScan.Server.Core.Module.Modules.Internal;

public class InternalUniScanServerPipelineConfigurator : IPipelineConfigurator
{
    public int Priority => 100;

    private readonly IServiceProvider _serviceProvider;
    
    public InternalUniScanServerPipelineConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void ConfigureFilters(IChannelPipeline pipeline)
    {
        pipeline.AddLast(nameof(AcceptedClientPacketFilter), _serviceProvider.GetRequiredService<AcceptedClientPacketFilter>());
    }

    public void ConfigureHandlers(IChannelPipeline pipeline)
    {
        pipeline.AddLast(nameof(ClientSoftwareInfoPacketHandler), _serviceProvider.GetRequiredService<ClientSoftwareInfoPacketHandler>());
        pipeline.AddLast(nameof(SubscribePacketHandler), _serviceProvider.GetRequiredService<SubscribePacketHandler>());
        pipeline.AddLast(nameof(GetDeviceListPacketHandler), _serviceProvider.GetRequiredService<GetDeviceListPacketHandler>());
    }
}