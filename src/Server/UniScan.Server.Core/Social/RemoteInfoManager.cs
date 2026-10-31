using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using ObservableCollections;
using R3;
using Shiki.Common.Extensions;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Util;
using Shiki.FileSystem.Abstractions;
using Shiki.FileSystem.Storage;
using Shiki.FileSystem.Storage.Directory.KeyValue;
using Shiki.FileSystem.Storage.Serializer;
using Shiki.Internationalization.Component;
using Shiki.Settings;
using Shiki.Settings.Util;
using UniScan.Core;
using UniScan.Core.Serialization;
using UniScan.Core.State.Node;
using UniScan.Network.Data.Info.Remote;

namespace UniScan.Server.Core.Social;

public class RemoteInfoManager
{
    public StoredObject<ObservableDictionary<Slug<SnakeSlugFormatter>, RemoteAnnouncement>> Announcements { get; }
    public StoredObject<SocialMetadata> Metadata { get; }
    public StoredObject<RemoteBranding> Branding { get; }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RemoteInfo GetRemoteInfo() => new(Metadata.Property.Value.ServerName, Metadata.Value.ServerDescription, Branding.Value, new RemoteSocial(Metadata.Value.MessageOfTheDay, Announcements.Value)); 

    public RemoteInfoManager(string directory, IPlatformFileManager fileManager, IPlatformDirectoryManager directoryManager, JsonSerializerOptions? serializerOptions)
    {
        JsonSerializerOptions jsonOptions = serializerOptions ?? JsonSerializerOptions.Default;
        
        var metadataStorage = new FileStorage<SocialMetadata>(directory, "metadata", directoryManager, fileManager, new JsonStorageSerializer<SocialMetadata>(jsonOptions));
        Metadata = new StoredObject<SocialMetadata>(new SocialMetadata("A UniScan Server",
                                                                       null
                                                                       , null),
                                                    metadataStorage, metadataStorage);
        // File.WriteAllText(Path.Combine(directory, "metadata.schema.json"), JsonSchemaUtil.GetSchema<SocialMetadata>(jsonOptions));
        
        var brandingStorage = new FileStorage<RemoteBranding>(directory, "branding", directoryManager, fileManager, new JsonStorageSerializer<RemoteBranding>(jsonOptions));
        Branding = new StoredObject<RemoteBranding>(new RemoteBranding(null, []),
                                                    brandingStorage, brandingStorage);
        // File.WriteAllText(Path.Combine(directory, "branding.schema.json"), JsonSchemaUtil.GetSchema<RemoteBranding>(jsonOptions));
        
        string announcementsDir = Path.Combine(directory, "announcements");
        // File.WriteAllText(Path.Combine(announcementsDir, "announcement.schema.json"), JsonSchemaUtil.GetSchema<RemoteAnnouncement>(jsonOptions));

        
        var directoryStorage = new DirectoryKeyValueStorage<RemoteAnnouncement>(announcementsDir, directoryManager, fileManager, new JsonStorageSerializer<RemoteAnnouncement>(jsonOptions));
        
        var m = new MappedBatchStorage<ObservableDictionary<Slug<SnakeSlugFormatter>, RemoteAnnouncement>, Dictionary<string, RemoteAnnouncement>, string>(directoryStorage, directoryStorage,
                 v => new ObservableDictionary<Slug<SnakeSlugFormatter>, RemoteAnnouncement>(
                  v.Select(
                           x => new KeyValuePair<Slug<SnakeSlugFormatter>, RemoteAnnouncement>(x.Key.ToSlug<SnakeSlugFormatter>(), x.Value)
                           )
                  ),
                 v => v.ToDictionary(x => x.Key.Value, x => x.Value));
        Announcements =
            new StoredObject<ObservableDictionary<Slug<SnakeSlugFormatter>, RemoteAnnouncement>>([], [v => v.ObserveChanged().Select(_ => v)], m, m);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task LoadAsync()
    {
        await Metadata.LoadAsync();
        await Announcements.LoadAsync();
        await Branding.LoadAsync();
    }
}

public record SocialMetadata([property: JsonPropertyName("server_name")] string? ServerName,
                             [property: JsonPropertyName("server_description")] ITextComponent? ServerDescription,
                             [property: JsonPropertyName("server_motd")] ITextComponent? MessageOfTheDay);