using System.Runtime.Versioning;
using Serilog;
using SpawnDev.SpawnJS;
using SpawnDev.SpawnJS.JSObjects;
using SpawnDev.SpawnJS.WebWorkers;
using UniScan.Platform.Filesystem;
using UniScan.Platform.Implementations.Web.Filesystem.Stream;
using File = SpawnDev.SpawnJS.JSObjects.File;

namespace UniScan.Platform.Implementations.Web.Filesystem;

[SupportedOSPlatform("browser")]
public class BrowserFileManager(WebWorkerService workerService, SpawnJSRuntime runtime, BrowserDirectoryManager directoryManager) : IPlatformFileManager
{
    public SpawnJSRuntime Runtime { get; } = runtime;
    public BrowserDirectoryManager DirectoryManager { get; } = directoryManager;

    public async Task<bool> ExistsAsync(string path) => await Get(path, false, false) != null;

    public async Task<FileSystemFileHandle?> Get(string path, bool createDirectories, bool createFile)
    {
        string filename = Path.GetFileName(path);
        string? p = Path.GetDirectoryName(path);

        FileSystemDirectoryHandle? dir;
        if (p != null)
        {
            dir = await DirectoryManager.Get(p, createDirectories);
        }
        else
        {
            dir = await DirectoryManager.GetRoot();
        }
        
        if (dir == null)
            return null;

        try
        {
            return await dir.GetFileHandle(filename, createFile);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to get file");
            return null;
        }
    }

    public async Task CopyAsync(string from, string to, bool overwrite) => await TransferAsync(from, to, false, overwrite);
    
    public async Task MoveAsync(string from, string to, bool overwrite) => await TransferAsync(from, to, true, overwrite);

    private async Task TransferAsync(string from, string to, bool move, bool overwrite)
    {
        FileSystemFileHandle? src = await Get(from, false, false);
        if (src == null)
        {
            throw new FileNotFoundException(from);
        }
        
        File srcFile = await src.GetFile();
        
        if (!overwrite && await ExistsAsync(to))
        {
            throw new InvalidOperationException($"File {to} already exists");
        }
        FileSystemFileHandle? dst = await Get(to, true, true);
        if (dst == null)
        {
            throw new DirectoryNotFoundException(Path.GetDirectoryName(to));
        }
        
        FileSystemWritableFileStream writable = await dst.CreateWritable();
        await writable.Write(srcFile);

        await writable.Close();

        if (move)
            await DeleteAsync(from);
    }
        
    public async Task<System.IO.Stream> GetStreamAsync(string path, FileMode mode, FileAccess access, FileShare share)
    {
        try
        {
            WebWorker? worker = await workerService.GetWebWorker();
            if (worker == null)
                throw new NullReferenceException();
            
            IOPFSWorkerService f = worker.GetService<IOPFSWorkerService>();
            int id = await f.OpenAsync(path, mode);
            
            return new OriginPrivateFileStream(f, id, await f.GetSizeAsync(id));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open file");
            throw;
        }
    }

    public async Task DeleteAsync(string path)
    {
        string? dirPath = Path.GetDirectoryName(path);
        FileSystemDirectoryHandle? dir = await DirectoryManager.GetDirOrRoot(dirPath);

        if (dir == null)
            throw new DirectoryNotFoundException(dirPath);
            
        await dir.RemoveEntry(Path.GetFileName(path));
    }
}