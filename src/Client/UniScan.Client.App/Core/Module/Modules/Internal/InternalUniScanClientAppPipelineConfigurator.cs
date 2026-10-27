using System;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;
using UniScan.Client.Core.Module.Modules.Internal.Handler;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.App.Core.Module.Modules.Internal;

public class InternalUniScanClientAppPipelineConfigurator(IServiceProvider serviceProvider) : IPipelineConfigurator
{
    public int Priority => 100;

    public void ConfigureFilters(IChannelPipeline pipeline)
    {
    }

    public void ConfigureHandlers(IChannelPipeline pipeline)
    {
        pipeline.AddLast(nameof(SetUISlotPacketHandler), serviceProvider.GetRequiredService<SetUISlotPacketHandler>());
        pipeline.AddLast(nameof(SetUINodePacketHandler), serviceProvider.GetRequiredService<SetUINodePacketHandler>());
    }
}