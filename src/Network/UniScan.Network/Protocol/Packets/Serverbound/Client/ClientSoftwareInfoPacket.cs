using MessagePack;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Protocol.Packets.Clientbound.Remote;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;
using UniScan.Network.Request;

namespace UniScan.Network.Protocol.Packets.Serverbound.Client;

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