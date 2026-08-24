using System.Text;
using System.Threading.Channels;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;
using Serilog;
using Shiki.Common.Identity;

namespace UniScan.Network.Packet;

public class PacketEncoder(PacketRegistry packetRegistry, MessagePackSerializerOptions? options = null, CancellationToken ct = default) : MessageToByteEncoder<IPacket>
{
    private readonly ILogger _logger = Log.ForContext<PacketEncoder>();

    protected override void Encode(IChannelHandlerContext context, IPacket message, IByteBuffer output)
    {
        Type type = message.GetType();
        Identifier? id = packetRegistry.GetIdentifier(type);
        if (id == null)
        {
            _logger.Error("Attempted to encode unregistered packet of type '{Type}'", type.FullName);
            context.CloseAsync();
            return;
        }
        
        byte[] str = Encoding.UTF8.GetBytes(id.ToString());
        if (str.Length > ushort.MaxValue)
        {
            _logger.Error("Packet identifier too long ({Length}/{MaxLength}): {Identifier}", str.Length, ushort.MaxValue, id.ToString());
            context.CloseAsync();
            return;
        }
        
        output.WriteUnsignedShortLE((ushort)str.Length);
        output.WriteBytes(str);

        try
        {
            byte[] msg = MessagePackSerializer.Serialize(type, message, options, ct);

            #if DEBUG
            var lines = msg.Chunk(16)
                           .Select(chunk => string.Join(" ", Convert.ToHexString(chunk).Chunk(2)
                                                                    .Select(c => new string(c))));

            Log.Debug("{lines}", string.Join(Environment.NewLine, lines));
            #endif

            output.WriteBytes(msg);
        }
        catch (FormatterNotRegisteredException ex)
        {
            _logger.Error(ex, "Attempted to serialize '{Type}' but no formatter was registered to handle it", message.GetType().FullName);
            context.CloseAsync();
        }
        catch (MessagePackSerializationException ex)
        {
            _logger.Error(ex, "Failed to serialize packet '{Type}'", message.GetType().FullName);
            context.CloseAsync();
        }
    }
}