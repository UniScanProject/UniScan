using System.ComponentModel;
using System.Text.Json.Serialization;
using MessagePack;

namespace UniScan.Network.Data.Info.Remote;

/// <summary>
/// Remote branding settings
/// </summary>
/// <param name="LogoUrl">The remote's logo URL, if present</param>
/// <param name="Links">A list of links to external websites</param>
[MessagePackObject]
public readonly record struct RemoteBranding(
    [property: Key(0), JsonPropertyName("logo_url")]
    Uri? LogoUrl,
    [property: Key(1), JsonPropertyName("links")]
    List<RemoteLink> Links
)
{
    public RemoteBranding() : this(null, []) { }
}