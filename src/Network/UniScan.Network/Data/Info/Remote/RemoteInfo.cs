using System.Collections.Immutable;
using System.Text.Json.Serialization;
using MessagePack;
using Shiki.Common.Identity;
using Shiki.Internationalization.Component;
using UniScan.Core.State.Node;

namespace UniScan.Network.Data.Info.Remote;

/// <summary>
/// Server info, sent to the client
/// </summary>
/// <param name="DisplayName">Server display name</param>
/// <param name="Description">Server description</param>
/// <param name="Branding">Branding (logo,links,etc)</param>
/// <param name="Social">Social stuff (announcements,motd)</param>
[MessagePackObject]
public record RemoteInfo(
    [property: Key(0), JsonPropertyName("display_name")] string? DisplayName,
    [property: Key(1), JsonPropertyName("description")] ITextComponent? Description,
    [property: Key(2), JsonPropertyName("branding"), JsonRequired] RemoteBranding Branding,
    [property: Key(3), JsonPropertyName("social"), JsonRequired] RemoteSocial Social
);