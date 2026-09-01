using System.Runtime.CompilerServices;
using UniScan.Platform.Filesystem;

namespace UniScan.Platform.Implementations.Native.Filesystem;

public class NativeDirectoryManager : IPlatformDirectoryManager
{
    public Task<bool> ExistsAsync(string path) => Task.FromResult(Directory.Exists(path));

    public Task CreateDirectoryAsync(string path) => Task.FromResult(Directory.CreateDirectory(path));
    
    public IAsyncEnumerable<string> EnumerateAsync(string path, string glob = "*", IPlatformDirectoryManager.DirectoryEnumerationType type = IPlatformDirectoryManager.DirectoryEnumerationType.ALL, [EnumeratorCancellation] CancellationToken ct = default)
    => type switch {
        IPlatformDirectoryManager.DirectoryEnumerationType.ALL         => Directory.EnumerateFileSystemEntries(path, glob).ToAsyncEnumerable(),
        IPlatformDirectoryManager.DirectoryEnumerationType.FILES       => Directory.EnumerateFiles(path, glob).ToAsyncEnumerable(),
        IPlatformDirectoryManager.DirectoryEnumerationType.DIRECTORIES => Directory.EnumerateDirectories(path, glob).ToAsyncEnumerable(),
        _                                                              => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public Task DeleteAsync(string path, bool recursive = false)
    {
        Directory.Delete(path, recursive);
        return Task.CompletedTask;
    }
}