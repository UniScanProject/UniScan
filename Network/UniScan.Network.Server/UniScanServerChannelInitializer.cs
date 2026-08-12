using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using UniScan.Network.Packet;
using UniScan.Network.Socket;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Network.Server;

public class UniScanServerChannelInitializer(
    PacketRegistry packetRegistry,
    IEnumerable<IPipelineConfigurator> configurators,
    IServiceProvider serviceProvider)
    : UniScanChannelInitializer(packetRegistry, configurators, serviceProvider)
{
    
    public override bool IsSharable { get; } = true;

    protected override void InitChannel(IChannel channel)
    {
        IChannelPipeline pipeline = channel.Pipeline;
        
        // framing
        pipeline.AddLast(new LengthFieldBasedFrameDecoder(DotNetty.Buffers.ByteOrder.LittleEndian, 1048576, 0, 4, 0, 4, true));
        pipeline.AddLast(new LengthFieldPrepender(DotNetty.Buffers.ByteOrder.LittleEndian, 4, 0, false));

        //our decoders
        pipeline.AddLast(new PacketDecoder(this._packetRegistry));
        pipeline.AddLast(new PacketEncoder(this._packetRegistry));
        
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