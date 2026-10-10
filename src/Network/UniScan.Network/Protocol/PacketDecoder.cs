using System.Buffers;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;
using Serilog;
using Shiki.Common.Identity;
using Shiki.Internationalization.Component;
using UniScan.Core.Extensions;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Registry;

namespace UniScan.Network.Protocol;

/// <summary>
/// Decodes packets into IPacket instances
/// </summary>
/// <param name="packetRegistry">The packet registry</param>
public class PacketDecoder(PacketRegistry packetRegistry) : MessageToMessageDecoder<IByteBuffer>
{
    private readonly ILogger _logger = Log.ForContext<PacketDecoder>();

    /// <inheritdoc/>
    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
        if (input.ReadableBytes < 2)
        {
            SendDisconnect(context, new TranslatableTextComponent("UniScan:i18n/disconnect/decoder_initiated/packet_too_small"));
            return;
        }
        
        ushort idLength = input.ReadUnsignedShortLE();
        if (input.ReadableBytes < idLength)
        {
            SendDisconnect(context, new TranslatableTextComponent("UniScan:i18n/disconnect/decoder_initiated/identifier_string_length_too_small"));
            return;
        }

        string ids = input.ReadString(idLength, Encoding.UTF8);

        var id = Identifier.TryParseIntoResult(ids);
        if (!id.HasValue)
        {
            _logger.Warning(id.Error, "Received packet with invalid Identifier string '{Id}', disconnecting.", ids);
            SendDisconnect(context, new TranslatableTextComponent("UniScan:i18n/disconnect/decoder_initiated/invalid_identifier"));
            
            return;
        }
        
        Type? type = packetRegistry.GetPacketType(id.Value);
        if (type == null)
        {
            _logger.Warning("Received unknown packet with ID {Id}, disconnecting.", id.Value);
            SendDisconnect(context, new TranslatableTextComponent("UniScan:i18n/disconnect/decoder_initiated/unrecognized_packet"));
            
            return;
        }

        byte[]? rented = null;
        int readable = input.ReadableBytes;

        try
        {
            //read messagepack data
            ReadOnlyMemory<byte> payload;
            if (input.HasArray)
            {
                //if we can, just reuse the array directly.
                //so we're not allocating entire new byte[] 
                payload = input.Array.AsMemory(input.ArrayOffset + input.ReaderIndex, readable);
                input.SkipBytes(readable);
            }
            else
            {
                //if not array for whatever reason, we'll rent an array to read into
                //thank god this isn't new york city
                rented = ArrayPool<byte>.Shared.Rent(readable);
                input.ReadBytes(rented, 0, readable);

                payload = rented.AsMemory(0, readable);
            }

#if DEBUG
            _logger.Debug("Received packet with identifier '{Identifier}' ({Length}): {Data}", id.Value, payload.Length,
                          payload.Span.ToHexViewString(HexViewStringArgs.DefaultConsole));
#endif

            //deserialize
            object? d = MessagePackSerializer.Deserialize(type, payload);
            if (d != null)
            {
                _logger.Debug("Received packet of type {Type}", d.GetType());
                output.Add(d);
            }
            else
            {
                _logger.Error("Received invalid packet of type '{Type}'", type.FullName);

                SendDisconnect(context, new TranslatableTextComponent("UniScan:i18n/disconnect/decoder_initiated/invalid_packet_data"));
            }
        }
        catch (FormatterNotRegisteredException ex)
        {
            _logger.Error(ex, "Failed to decode packet of type '{Type}' as it has not been registered", type.FullName);

            SendDisconnect(context, new TranslatableTextComponent("UniScan:i18n/disconnect/decoder_initiated/unregistered_packet_deserializer"));
        }
        catch (MessagePackSerializationException ex)
        {
            _logger.Error(ex, "Failed to decode packet of type '{Type}'", type.FullName);

            SendDisconnect(context, new TranslatableTextComponent("UniScan:i18n/disconnect/decoder_initiated/packet_deserialization_fail"));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to decode packet");

            SendDisconnect(context, new TranslatableTextComponent("UniScan:i18n/disconnect/decoder_initiated/generic"));
        }
        finally
        {
            if (rented != null)
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    private static void SendDisconnect(IChannelHandlerContext context, ITextComponent reason)
    {
        context.Channel.WriteAndFlushAsync(new DisconnectPacket(reason)).ContinueWith(_ => context.CloseAsync(), TaskContinuationOptions.ExecuteSynchronously);
    }
}