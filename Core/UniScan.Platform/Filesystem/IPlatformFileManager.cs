namespace UniScan.Platform.Filesystem;

public interface IPlatformFileManager
{
    public Task<bool> ExistsAsync(string path);
    public Task CopyAsync(string from, string to, bool overwrite);
    public Task<Stream> GetStreamAsync(string path, FileMode mode, FileAccess access, FileShare share);
}