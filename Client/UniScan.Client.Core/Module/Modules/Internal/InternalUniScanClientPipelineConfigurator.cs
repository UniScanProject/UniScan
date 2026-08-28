using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;
using UniScan.Client.Core.Config.Remote;
using UniScan.Client.Core.Module.Modules.Internal.Handler;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.Core.Module.Modules.Internal;

public class InternalUniScanClientPipelineConfigurator(IServiceProvider serviceProvider) : IPipelineConfigurator
{
    public int Priority => 100;

    public void ConfigureFilters(IChannelPipeline pipeline)
    {
    }

    public void ConfigureHandlers(IChannelPipeline pipeline)
    {
        pipeline.AddLast(nameof(DisconnectPacketHandler), serviceProvider.GetRequiredService<DisconnectPacketHandler>());
        pipeline.AddLast(nameof(RemoteInfoPacketHandler), serviceProvider.GetRequiredService<RemoteInfoPacketHandler>());

    }
}