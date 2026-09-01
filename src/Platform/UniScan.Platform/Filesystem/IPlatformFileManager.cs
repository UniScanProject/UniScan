namespace UniScan.Platform.Filesystem;

public interface IPlatformFileManager : IPlatformFileSystemManager
{
    public Task CopyAsync(string from, string to, bool overwrite);
    public Task MoveAsync(string from, string to, bool overwrite);

    Task DeleteAsync(string path);

    public Task<Stream> GetStreamAsync(string path, FileMode mode, FileAccess access, FileShare share);
}