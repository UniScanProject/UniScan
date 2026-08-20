namespace UniScan.Platform.Filesystem;

public interface IPlatformDirectoryManager
{
    public Task<bool> ExistsAsync(string path);
    public Task CreateDirectoryAsync(string path);
}