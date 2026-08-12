using MessagePack;
using UniScan.Network.Data.Info.Remote;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound.Remote;

/// <summary>
/// Sent to clients that request, can be sent before a client has authenticated.
/// </summary>
[RegistryPacket("UniScan", "packet", "clientbound", "remote", "info")]
[MessagePackObject]
public readonly record struct RemoteInfoPacket(
    [property: Key(0)] RemoteInfo Info,
    [property: Key(1)] Guid? RequestId
) : IClientboundPacket, IResponsePayloadPart;