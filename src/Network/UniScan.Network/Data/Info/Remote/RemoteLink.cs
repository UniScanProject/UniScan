using MessagePack;

namespace UniScan.Network.Data.Info.Remote;

[MessagePackObject]
public readonly record struct RemoteLink(
    [property: Key(0)] Uri? IconUrl,
    [property: Key(1)] string Name,
    [property: Key(2)] Uri Url
);