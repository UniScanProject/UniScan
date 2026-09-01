using MessagePack;

namespace UniScan.Network.Data.Info.Remote;

//TODO in future, refer to a User instead
//and then let the remote provide a good enough API to return info on the user
[MessagePackObject]
public readonly record struct RemoteAnnouncementAuthor(
    [property: Key(0)] string DisplayName,
    [property: Key(1)] Uri? Avatar
);

[MessagePackObject]
public readonly record struct RemoteAnnouncement(
    [property: Key(0)] string Title,
    [property: Key(1)] string Body,
    [property: Key(2)] DateTimeOffset Published,
    [property: Key(3)] List<DateTimeOffset> Edits,
    [property: Key(4)] List<RemoteAnnouncementAuthor> Authors
);