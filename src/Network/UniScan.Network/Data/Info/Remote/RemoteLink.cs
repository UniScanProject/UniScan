using System.Text.Json.Serialization;
using MessagePack;
using Shiki.Internationalization.Component;

namespace UniScan.Network.Data.Info.Remote;

/// <summary>
/// A link to an external website
/// </summary>
/// <param name="IconUrl">The icon url, if present</param>
/// <param name="Name">The link title</param>
/// <param name="Url">The link itself</param>
[MessagePackObject]
public readonly record struct RemoteLink(
    [property: Key(0), JsonPropertyName("icon_url")] Uri? IconUrl,
    [property: Key(1), JsonPropertyName("name")] ITextComponent Name,
    [property: Key(2), JsonPropertyName("url")] Uri Url
);