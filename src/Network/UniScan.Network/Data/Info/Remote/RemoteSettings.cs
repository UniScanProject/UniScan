using MessagePack;

namespace UniScan.Network.Data.Info.Remote;

[MessagePackObject]
public readonly record struct RemoteSettings(
    [property: Key(0)] bool AllowsAnonymousLogin
);