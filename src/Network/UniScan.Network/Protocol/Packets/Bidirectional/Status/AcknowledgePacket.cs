using MessagePack;
using Shiki.Common.Result;
using Shiki.Extensions.MessagePack.Formatter.Result;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;
using UniScan.Network.Request;

namespace UniScan.Network.Protocol.Packets.Bidirectional.Status;

[RegistryPacket("UniScan", "packet", "bidirectional", "status", "ack")]
[MessagePackObject]
[method: RequestConstructor]
public readonly record struct AcknowledgePacket(
    [property: Key(0), MessagePackFormatter(typeof(TransportableBooleanResultMessagePackFormatter))]
    TransportableBooleanResult Result,
    [property: Key(1)] Guid? RequestId = null) : IBidirectionalPacket, IResponsePayloadPart
{
    public static readonly AcknowledgePacket Success = new(TransportableBooleanResult.Successful);
}