using System.Text.Json.Serialization;
using MessagePack;

namespace UniScan.Network.Data.Info.Remote;

//TODO in future, refer to a User instead
//and then let the remote provide a good enough API to return info on the user
[MessagePackObject]
public readonly record struct RemoteAnnouncementAuthor(
    [property: Key(0), JsonPropertyName("display_name")] string DisplayName,
    [property: Key(1), JsonPropertyName("avatar_url")] Uri? Avatar
);

[MessagePackObject]
public readonly record struct RemoteAnnouncement(
    [property: Key(0), JsonPropertyName("title")] string Title,
    [property: Key(1), JsonPropertyName("body")] string Body,
    [property: Key(2), JsonPropertyName("published_date")] DateTimeOffset Published,
    [property: Key(3), JsonPropertyName("edit_dates")] List<DateTimeOffset> Edits,
    [property: Key(4), JsonPropertyName("authors")] List<RemoteAnnouncementAuthor> Authors
);