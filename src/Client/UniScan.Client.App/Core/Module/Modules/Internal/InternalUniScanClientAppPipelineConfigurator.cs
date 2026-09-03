using System;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;
using UniScan.Client.App.Core.Module.Modules.Internal.Handler;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.App.Core.Module.Modules.Internal;

public class InternalUniScanClientAppPipelineConfigurator : IPipelineConfigurator
{
    public int Priority => 100;

    private readonly IServiceProvider _serviceProvider;
    
    public InternalUniScanClientAppPipelineConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void ConfigureFilters(IChannelPipeline pipeline)
    {
    }

    public void ConfigureHandlers(IChannelPipeline pipeline)
    {
        pipeline.AddLast(nameof(SetUISlotPacketHandler), _serviceProvider.GetRequiredService<SetUISlotPacketHandler>());
    }
}