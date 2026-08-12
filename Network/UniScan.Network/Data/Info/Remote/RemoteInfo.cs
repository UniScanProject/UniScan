using MessagePack;
using UniScan.Network.Packet.Packets.Clientbound.Remote;

namespace UniScan.Network.Data.Info.Remote;

[MessagePackObject]
public readonly record struct RemoteInfo(
    [property: Key(0)] string DisplayName,
    [property: Key(1)] string? Description,
    [property: Key(2)] RemoteSettings Settings,
    [property: Key(3)] RemoteBranding Branding,
    [property: Key(4)] RemoteSocial Social
);