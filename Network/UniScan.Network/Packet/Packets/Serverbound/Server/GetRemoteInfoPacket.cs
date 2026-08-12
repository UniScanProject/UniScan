using MessagePack;
using UniScan.Network.Packet.Packets.Clientbound.Remote;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Serverbound.Server;

[MessagePackObject]
[RegistryPacket("UniScan", "packet", "serverbound", "remote", "get_device_list")]
public readonly record struct GetRemoteInfoPacket(
    [property: Key(0)] Guid? RequestId
) : IServerboundPacket, IRequestPayloadPart<RemoteInfoPacket>;