using MessagePack;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;

namespace UniScan.Network.Protocol.Packets.Clientbound.Remote;

/// <summary>
/// Sent to clients so that they can tell whether to disconnect due to version differences.
///
/// Should stay the same throughout versions, attributes are added in the RemoteInfoPacket instead.
/// Clients should get this packet before getting the RemoteInfoPacket to prevent connecting to an incompatible server.
/// </summary>
[RegistryPacket("UniScan", "packet", "clientbound", "remote", "software_info")]
[MessagePackObject]
public readonly record struct ServerSoftwareInfoPacket(
    [property: Key(0)] ServerSoftwareInfo Info,
    [property: Key(1)] Guid? RequestId
) : IClientboundPacket, IResponsePayloadPart;