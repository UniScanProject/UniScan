using MessagePack;
using Shiki.Common.Result;
using Shiki.Extensions.MessagePack.Formatter.Result;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;

namespace UniScan.Network.Protocol.Packets.Bidirectional.Status;

[RegistryPacket("UniScan", "packet", "bidirectional", "status", "ack")]
[MessagePackObject]
public readonly record struct AcknowledgePacket(
    [property: Key(0), MessagePackFormatter(typeof(TransportableBooleanResultMessagePackFormatter))] TransportableBooleanResult Result,
    [property: Key(1)] Guid? RequestId
) : IBidirectionalPacket, IResponsePayloadPart;