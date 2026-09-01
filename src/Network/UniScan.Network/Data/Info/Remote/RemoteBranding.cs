using MessagePack;

namespace UniScan.Network.Data.Info.Remote;

[MessagePackObject]
public readonly record struct RemoteBranding(
    [property: Key(0)] Uri? LogoUrl,
    [property: Key(1)] List<RemoteLink> Links
);