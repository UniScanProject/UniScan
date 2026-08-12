using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;
using UniScan.Client.Core.Module.Modules.Internal.Handler;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.Core.Module.Modules.Internal;

public class InternalUniScanClientPipelineConfigurator : IPipelineConfigurator
{
    public int Priority => 100;

    private readonly IServiceProvider _serviceProvider;
    
    public InternalUniScanClientPipelineConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void ConfigureFilters(IChannelPipeline pipeline)
    {
    }

    public void ConfigureHandlers(IChannelPipeline pipeline)
    {
        pipeline.AddLast(nameof(DisconnectPacketHandler), _serviceProvider.GetRequiredService<DisconnectPacketHandler>());
    }
}