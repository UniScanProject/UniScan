using Microsoft.Extensions.Logging;
using UniScan.Client.Core.Storage.Serializer;
using UniScan.Platform.Filesystem;

namespace UniScan.Client.Core.Storage;

public class DirectoryStorage<TValue>(string directory, IPlatformDirectoryManager directoryManager, IPlatformFileManager fileManager, DirectoryStorage<TValue>.ValueToNameResolver resolver, IStorageFileSerializer<TValue> serializer, ILogger<DirectoryKeyValueStorage<TValue>> logger)
    : BaseDirectoryStorage(directory), IStorage<List<TValue>>
{
    public delegate string ValueToNameResolver(TValue value);

    public async Task<List<TValue>?> LoadAsync()
    {
        if (!await directoryManager.ExistsAsync(Directory))
            return null;

        List<TValue> d = [];
        await foreach (string file in directoryManager.EnumerateAsync(Directory, serializer.FileExtensionGlob, IPlatformDirectoryManager.DirectoryEnumerationType.FILES))
        {
            try
            {
                await using Stream fs = await fileManager.GetStreamAsync(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                TValue? data = await serializer.DeserializeAsync(fs);

                if (data != null)
                {
                    d.Add(data);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to deserialize file '{File}'", file);
            }
        }

        return d;
    }

    public async Task SaveAsync(List<TValue>? data)
    {
        if (data is null)
            return;
        
        if (!await directoryManager.ExistsAsync(Directory))
        {
            await directoryManager.CreateDirectoryAsync(Directory);
        }

        foreach (TValue d in data)
        {
            string path = Path.Combine(Directory, $"{resolver(d)}.{serializer.FileExtension}");

            try
            {
                await using Stream fs = await fileManager.GetStreamAsync(path, FileMode.Create, FileAccess.Write, FileShare.None);
                await serializer.SerializeAsync(fs, d);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to save file '{File}'", path);
            }
        }
    }
}