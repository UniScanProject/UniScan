using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using UniScan.Network.Protocol;
using UniScan.Network.Registry;
using UniScan.Network.Socket;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Network.Client;

public class UniScanClientChannelInitializer(
    PacketRegistry packetRegistry,
    IEnumerable<IPipelineConfigurator> configurators,
    IServiceProvider serviceProvider)
    : UniScanChannelInitializer(packetRegistry, configurators, serviceProvider)
{
    protected override void InitChannel(IChannel channel)
    {
        IChannelPipeline pipeline = channel.Pipeline;
        
        //our decoders
        pipeline.AddLast(new PacketDecoder(_packetRegistry));
        pipeline.AddLast(new PacketEncoder(_packetRegistry));
        
        //filters
        foreach (IPipelineConfigurator configurator in _configurators)
        {
            configurator.ConfigureFilters(pipeline);
        }
        
        //handlers
        foreach (IPipelineConfigurator configurator in _configurators)
        {
            configurator.ConfigureHandlers(pipeline);
        }
    }
}