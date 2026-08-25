using UniScan.Platform.Filesystem;

namespace UniScan.Platform.Implementations.Native.Filesystem;

public class NativeDirectoryManager : IPlatformDirectoryManager
{
    public Task<bool> ExistsAsync(string path) => Task.FromResult(Directory.Exists(path));

    public Task CreateDirectoryAsync(string path) => Task.FromResult(Directory.CreateDirectory(path));
    public Task<IEnumerable<string>> EnumerateFilesAsync(string path, string glob = "*") => Task.FromResult(Directory.EnumerateFiles(path, glob));
}