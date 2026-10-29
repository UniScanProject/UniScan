using MessagePack;
using MessagePack.Formatters;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Extensions.MessagePack.Formatter.Identity;
using Shiki.Internationalization.Component;

namespace UniScan.Network.Data.Info.Remote;

[MessagePackObject]
public readonly record struct RemoteSocial(
    [property: Key(0)] ITextComponent? MessageOfTheDay,
    [property: Key(1)] IReadOnlyDictionary<Slug<SnakeSlugFormatter>, RemoteAnnouncement> Announcements//TODO make this list of IDs, so that we aren't sending such a large packet if there are many announcements
);