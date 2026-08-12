using MessagePack;

namespace UniScan.Network.Data.Info.Remote;

[MessagePackObject]
public readonly record struct RemoteAnnouncement(
    [property: Key(0)] string Title,
    [property: Key(1)] string Body,
    [property: Key(2)] DateTimeOffset Published,
    [property: Key(3)] List<DateTimeOffset> Edits
);