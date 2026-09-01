using Microsoft.Extensions.Logging;
using UniScan.Client.Core.Storage.Serializer;
using UniScan.Platform.Filesystem;

namespace UniScan.Client.Core.Storage;

public class FileStorage<T>(string? directory, string filename, IPlatformDirectoryManager directoryManager, IPlatformFileManager fileManager, IStorageSerializer<T> serializer, ILogger<FileStorage<T>> logger) : IStorage<T>
where T : class
{
    private readonly ILogger<FileStorage<T>> _logger = logger;

    private string FullFilename => serializer is IStorageFileSerializer<T> s
                                       ? $"{System.IO.Path.GetFileNameWithoutExtension(filename)}.{s.FileExtension}"
                                       : filename;
    private string Path => System.IO.Path.Combine(directory ?? ".", FullFilename);

    public FileStorage(string path, IPlatformDirectoryManager directoryManager, IPlatformFileManager fileManager, IStorageSerializer<T> serializer, ILogger<FileStorage<T>> logger) :  this(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileName(path), directoryManager, fileManager, serializer, logger)
    {
    }

    public async Task<T?> LoadAsync()
    {
        if (!await fileManager.ExistsAsync(Path))
        {
            return null;
        }

        try
        {
            await using Stream fs = await fileManager.GetStreamAsync(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await serializer.DeserializeAsync(fs);
        }
        catch (Exception)
        {
            _logger.LogError("Could not deserialize file {Path}", Path);//TODO this should throw....
        }

        return null;
    }

    public async Task SaveAsync(T? data)
    {
        if (data is null)
        {
            return;
        }
        
        if (directory != null)
        {
            if (!await directoryManager.ExistsAsync(directory))
            {
                await directoryManager.CreateDirectoryAsync(directory);
            }
        }

        try
        {
            await using Stream fs = await fileManager.GetStreamAsync(Path, FileMode.Create, FileAccess.Write, FileShare.None);
            await serializer.SerializeAsync(fs, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not save file {Path}", Path);
        }
    }
}