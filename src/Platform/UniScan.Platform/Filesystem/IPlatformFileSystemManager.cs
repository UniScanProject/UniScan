namespace UniScan.Platform.Filesystem;

public interface IPlatformFileSystemManager
{
    public Task<bool> ExistsAsync(string path);
}