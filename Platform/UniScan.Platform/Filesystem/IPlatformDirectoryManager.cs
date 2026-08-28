using System.Runtime.CompilerServices;

namespace UniScan.Platform.Filesystem;

public interface IPlatformDirectoryManager : IPlatformFileSystemManager
{
    public Task CreateDirectoryAsync(string path);

    public enum DirectoryEnumerationType
    {
        ALL,
        FILES,
        DIRECTORIES
    }
    
    public IAsyncEnumerable<string> EnumerateAsync(string path, string glob = "*", DirectoryEnumerationType type = DirectoryEnumerationType.ALL, [EnumeratorCancellation] CancellationToken ct = default);

    public Task DeleteAsync(string path, bool recursive = false);
}