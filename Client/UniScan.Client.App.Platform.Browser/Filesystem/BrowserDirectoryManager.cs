using Serilog;
using SpawnDev.SpawnJS;
using SpawnDev.SpawnJS.JSObjects;
using UniScan.Platform.Filesystem;

namespace UniScan.Client.App.Platform.Browser.Filesystem;

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
    
    public async Task<FileSystemDirectoryHandle> GetRoot() => await Navigator.Storage.GetDirectory();

    public async Task<FileSystemDirectoryHandle> GetExistingDirOrRoot(string? path) => path != null ? await Get(path.Split([Path.PathSeparator, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries), false) ?? await GetRoot() : await GetRoot();
    
    public async Task<FileSystemDirectoryHandle?> GetDirOrRoot(string? path) => path != null ? await Get(path.Split([Path.PathSeparator, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries), false) : await GetRoot();
    
    public async Task<FileSystemDirectoryHandle?> Get(string path, bool create) => await Get(path.Split([Path.PathSeparator, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries), create);
    
    public async Task<FileSystemDirectoryHandle?> Get(string[] parts, bool create)
    {
        try
        {
            FileSystemDirectoryHandle current = await GetRoot(); //root
            foreach (string part in parts)
            {
                Log.Debug("cur dir: {cur}", current.Name);
                current = await current.GetDirectoryHandle(part, create);
            }

            return current;
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Directory not found!!!");
            return null;
        }
    }
}