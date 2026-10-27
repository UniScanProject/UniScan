using System.Collections.Immutable;
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
    [property: Key(0)] string DisplayName,
    [property: Key(1)] ITextComponent? Description,
    [property: Key(2)] RemoteBranding Branding,
    [property: Key(3)] RemoteSocial Social
);