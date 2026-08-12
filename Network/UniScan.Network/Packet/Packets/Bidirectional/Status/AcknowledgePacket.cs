using MessagePack;
using Shiki.Common.Result;
using Shiki.Extensions.MessagePack.Formatter.Result;
using UniScan.Network.Packet.PayloadPart;
using UniScan.Network.Request;

namespace UniScan.Network.Packet.Packets.Bidirectional.Status;

[RegistryPacket("UniScan", "packet", "bidirectional", "status", "ack")]
[MessagePackObject]
public readonly record struct AcknowledgePacket(
    [property: Key(0), MessagePackFormatter(typeof(TransportableBooleanResultFormatter))] TransportableBooleanResult Result,
    [property: Key(1)] Guid? RequestId
) : IBidirectionalPacket, IResponsePayloadPart;