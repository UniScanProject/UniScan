using System.IO.Enumeration;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Serilog;
using SpawnDev.SpawnJS;
using SpawnDev.SpawnJS.JSObjects;
using UniScan.Platform.Filesystem;

namespace UniScan.Platform.Implementations.Web.Filesystem;

[SupportedOSPlatform("browser")]
public class BrowserDirectoryManager : IPlatformDirectoryManager
{
    public SpawnJSRuntime Runtime { get; }

    public Navigator Navigator { get; }

    public BrowserDirectoryManager(SpawnJSRuntime runtime)
    {
        Runtime = runtime;

        Navigator = Runtime.Get<Navigator>("navigator");
    }

    public async Task<bool> ExistsAsync(string path) => await Get(path, false) != null;

    public async Task CreateDirectoryAsync(string path) => await Get(path, true);

    public async IAsyncEnumerable<string> EnumerateAsync(string path, string glob = "*",
                                                         IPlatformDirectoryManager.DirectoryEnumerationType type =
                                                             IPlatformDirectoryManager.DirectoryEnumerationType.ALL,
                                                         [EnumeratorCancellation] CancellationToken ct = default)
    {
        FileSystemDirectoryHandle? handle = await Get(path, false);
        if (handle == null)
            throw new DirectoryNotFoundException();

        await foreach ((string, FileSystemHandle) entry in handle.EntriesEnumerable().WithCancellation(ct))
        {
            if (!FileSystemName.MatchesSimpleExpression(glob, entry.Item1))
                continue;

            switch (entry.Item2.Kind)
            {
                case "file" when type      != IPlatformDirectoryManager.DirectoryEnumerationType.DIRECTORIES:
                case "directory" when type != IPlatformDirectoryManager.DirectoryEnumerationType.FILES:
                    yield return Path.Combine(path, entry.Item1);
                    break;
            }
        }
    }

    public async Task DeleteAsync(string path, bool recursive = false)
    {
        string name = Path.GetFileName(path);
        string? parent = Path.GetDirectoryName(path);

        FileSystemDirectoryHandle? handle = parent != null ? await Get(parent, false) : await GetRoot();
        if (handle == null)
            throw new DirectoryNotFoundException();

        await handle.RemoveEntry(name, recursive);
    }

    public async Task<FileSystemDirectoryHandle> GetRoot() => await Navigator.Storage.GetDirectory();

    public async Task<FileSystemDirectoryHandle> GetExistingDirOrRoot(string? path) =>
        path != null
            ? await Get(path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                                   StringSplitOptions.RemoveEmptyEntries), false) ?? await GetRoot()
            : await GetRoot();

    public async Task<FileSystemDirectoryHandle?> GetDirOrRoot(string? path) =>
        path != null
            ? await Get(path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                                   StringSplitOptions.RemoveEmptyEntries), false)
            : await GetRoot();

    public async Task<FileSystemDirectoryHandle?> Get(string path, bool create) =>
        await Get(path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                             StringSplitOptions.RemoveEmptyEntries), create);

    public async Task<FileSystemDirectoryHandle?> Get(string[] parts, bool create)
    {
        try
        {
            FileSystemDirectoryHandle current = await GetRoot(); //root
            foreach (string part in parts)
            {
#if DEBUG
                Log.Debug("cur dir: {cur}", current.Name);
#endif
                current = await current.GetDirectoryHandle(part, create);
            }

            return current;
        }
        catch (Exception ex)
        {
#if DEBUG
            Log.Debug(ex, "Directory not found!!!");
#endif
            return null;
        }
    }
}