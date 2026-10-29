using MessagePack;
using Shiki.Internationalization.Component;

namespace UniScan.Network.Data.Info.Remote;

[MessagePackObject]
public readonly record struct RemoteLink(
    [property: Key(0)] Uri? IconUrl,
    [property: Key(1)] ITextComponent Name,
    [property: Key(2)] Uri Url
);