using Microsoft.Extensions.Logging;
using UniScan.Client.Core.Storage.Serializer;
using UniScan.Platform.Filesystem;

namespace UniScan.Client.Core.Storage;

public class BaseDirectoryStorage(string directory)
{
    protected readonly string Directory = directory;
}

public class DirectoryKeyValueStorage<TValue>(string directory, IPlatformDirectoryManager directoryManager, IPlatformFileManager fileManager, IStorageFileSerializer<TValue> serializer, ILogger<DirectoryKeyValueStorage<TValue>> logger)
    : BaseDirectoryStorage(directory), IStorage<Dictionary<string, TValue>>
{
    private readonly ILogger<DirectoryKeyValueStorage<TValue>> _logger = logger;

    public async Task<Dictionary<string, TValue>?> LoadAsync()
    {
        if (!System.IO.Directory.Exists(Directory))
            return null;

        Dictionary<string, TValue> d = [];
        foreach (string file in await directoryManager.EnumerateFilesAsync(Directory, serializer.FileExtensionGlob))
        {
            try
            {
                await using FileStream fs = new(file, FileMode.Open, FileAccess.Read);
                TValue? data = await serializer.DeserializeAsync(fs);

                if (data != null)
                {
                    d.Add(Path.GetFileNameWithoutExtension(file), data);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to deserialize file '{File}'", file);
            }
        }

        return d;
    }

    public async Task SaveAsync(Dictionary<string, TValue>? data)
    {
        if (data is null)
            return;
        
        if (!System.IO.Directory.Exists(Directory))
        {
            System.IO.Directory.CreateDirectory(Directory);
        }

        foreach (KeyValuePair<string, TValue> d in data)
        {
            string path = Path.Combine(Directory, $"{d.Key}.{serializer.FileExtension}");

            try
            {
                await using FileStream fs = new(path, FileMode.Create, FileAccess.Write);
                await serializer.SerializeAsync(fs, d.Value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to save file '{File}'", path);
            }
        }
    }
    
    public async Task SaveAsync(string id, TValue data)
    {
        if (data is null)
            return;
        
        if (!System.IO.Directory.Exists(Directory))
        {
            System.IO.Directory.CreateDirectory(Directory);
        }
        
        string path = Path.Combine(Directory, $"{id}.{serializer.FileExtension}");

        try
        {
            await using FileStream fs = new(path, FileMode.Create, FileAccess.Write);
            await serializer.SerializeAsync(fs, data);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to save file '{File}'", path);
        }
    }

    public async Task DeleteAsync(string id)
    {
        if (!await directoryManager.ExistsAsync(Directory))
            return;
        
        string path = Path.Combine(Directory, $"{id}.{serializer.FileExtension}");

        if (!await fileManager.ExistsAsync(path))
            return;

        await fileManager.DeleteAsync(path);
    }
}