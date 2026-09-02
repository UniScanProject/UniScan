using MessagePack;
using UniScan.Network.Data.Info.Remote;
using UniScan.Network.Registry;

namespace UniScan.Network.Protocol.Packets.Clientbound.Remote;

/// <summary>
/// Sent to clients after receiving ClientSoftwareInfo
/// </summary>
[RegistryPacket("UniScan", "packet", "clientbound", "remote", "info")]
[MessagePackObject]
public readonly record struct RemoteInfoPacket(
    [property: Key(0)] RemoteInfo Info
) : IClientboundPacket;