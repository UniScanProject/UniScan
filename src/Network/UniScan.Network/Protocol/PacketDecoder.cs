using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;
using Serilog;
using Shiki.Common.Identity;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Registry;

namespace UniScan.Network.Protocol;

public class PacketDecoder(PacketRegistry packetRegistry) : ByteToMessageDecoder
{
    private readonly ILogger _logger = Log.ForContext<PacketDecoder>();

    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
        if (input.ReadableBytes < 2) return;
        
        input.MarkReaderIndex();
        
        short idLength = input.ReadShortLE();
        if (input.ReadableBytes < idLength)
        {
            input.ResetReaderIndex();
            return;
        }

        string ids = input.ReadString(idLength, Encoding.UTF8);

        var id = Identifier.TryParseIntoResult(ids);
        if (!id.HasValue)
        {
            _logger.Warning(id.Error, "Received packet with invalid Identifier string '{Id}', disconnecting.", ids);
            
            input.SkipBytes(input.ReadableBytes);
            context.WriteAndFlushAsync(new DisconnectPacket("Received packet with invalid Identifier.")).ContinueWith(_ => context.CloseAsync());
            return;
        }
        
        Type? type = packetRegistry.GetPacketType(id.Value);
        if (type == null)
        {
            _logger.Warning("Received unknown packet with ID {Id}, disconnecting.", id.Value);
            
            //skip
            input.SkipBytes(input.ReadableBytes); 
            context.WriteAndFlushAsync(new DisconnectPacket("Received unrecognized packet.")).ContinueWith(_ => context.CloseAsync());
            return;
        }
        
        byte[] payload = new byte[input.ReadableBytes];
        input.ReadBytes(payload);

        try
        {
            object? d = MessagePackSerializer.Deserialize(type, payload);
            if (d != null)
            {
                _logger.Debug("Received packet of type {Type}", d.GetType());
                output.Add(d);
            }
            else
            {
                _logger.Error("Received invalid packet of type '{Type}'", type.FullName);

                context.WriteAndFlushAsync(new DisconnectPacket("Received invalid packet."))
                       .ContinueWith(_ => context.CloseAsync());
            }
        }
        catch (MessagePackSerializationException ex)
        {
            _logger.Error(ex, "Failed to decode packet of type '{Type}'", type.FullName);
            
            context.WriteAndFlushAsync(new DisconnectPacket("Received invalid packet.")).ContinueWith(_ => context.CloseAsync());
        }
    }
}