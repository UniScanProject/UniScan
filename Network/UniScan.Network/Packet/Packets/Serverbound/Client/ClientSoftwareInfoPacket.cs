using MessagePack;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Packet.Packets.Bidirectional.Status;
using UniScan.Network.Packet.Packets.Clientbound.Remote;
using UniScan.Network.Packet.PayloadPart;
using UniScan.Network.Request;

namespace UniScan.Network.Packet.Packets.Serverbound.Client;

/// <summary>
/// Sent to servers so that they can tell whether to disconnect due to version differences.
///
/// Should stay the same throughout versions, attributes are added in the RemoteInfoPacket instead.
/// Servers should be sent this packet before trying to process other packets from clients.
/// </summary>
[RegistryPacket("UniScan", "packet", "serverbound", "client", "software_info")]
[MessagePackObject]
[method: RequestConstructor]
public readonly partial record struct ClientSoftwareInfoPacket(
    [property: Key(0)] ClientSoftwareInfo Info,
    [property: Key(1)] Guid? RequestId
) : IServerboundPacket, IRequestPayloadPart<ServerSoftwareInfoPacket>;