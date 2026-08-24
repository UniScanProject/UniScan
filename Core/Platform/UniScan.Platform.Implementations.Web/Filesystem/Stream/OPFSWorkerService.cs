using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using SpawnDev.SpawnJS.JSObjects;
using Array = System.Array;

namespace UniScan.Platform.Implementations.Web.Filesystem.Stream;

[SupportedOSPlatform("browser")]
public class OPFSWorkerService(BrowserFileManager fileManager) : IOPFSWorkerService
{
    private OPFSHandleStorage _handles = new();
    
    public async Task<int> OpenAsync(string path, FileMode mode)
    {
        if (mode == FileMode.CreateNew && await fileManager.ExistsAsync(path)) {
            throw new InvalidOperationException("File already exists");
        }
        
        bool c = mode is FileMode.Create or FileMode.CreateNew or FileMode.OpenOrCreate;
        FileSystemFileHandle? file = await fileManager.Get(path, c, c);

        if (file == null) throw new InvalidOperationException("Failed to open file");
        
        FileSystemSyncAccessHandle handle = await file.CreateSyncAccessHandle();

        if (mode is FileMode.Create or FileMode.Truncate)
        {
            handle.Truncate(0);
            handle.Flush();
        }
        
        return _handles.RegisterHandle(handle);
    }
    
    //I can marshal fucking tuple just fine but not custom class??????
    public async Task<(byte[] Buffer, long Length)> ReadAsync(int id, long offset, int count)
    {
        FileSystemSyncAccessHandle handle = _handles.GetHandle(id);
        
        byte[] buffer = new byte[count];
        long read = handle.Read(buffer, new FileSystemSyncReadWriteOptions { At = offset });
        
        return (buffer, read);
    }
    
    public Task<long> WriteAsync(int id, byte[] buffer, int offset)
    {
        FileSystemSyncAccessHandle handle = _handles.GetHandle(id);
        
        return Task.FromResult(handle.Write(buffer, new FileSystemSyncReadWriteOptions { At = offset }));
    }

    public Task FlushAsync(int id)
    {
        FileSystemSyncAccessHandle handle = _handles.GetHandle(id);
        
        handle.Flush();

        return Task.CompletedTask;
    }

    public Task TruncateAsync(int id, long c)
    {
        FileSystemSyncAccessHandle handle = _handles.GetHandle(id);
        
        handle.Truncate(c);

        return Task.CompletedTask;
    }

    public Task CloseAsync(int id)
    {
        _handles.UnregisterHandle(id);

        return Task.CompletedTask;
    }
    
    public Task<long> GetSizeAsync(int id)
    {
        FileSystemSyncAccessHandle handle = _handles.GetHandle(id);
        
        return Task.FromResult(handle.GetSize());
    }
}

[SupportedOSPlatform("browser")]
public class OPFSHandleStorage
{
    //I don't like this
    private readonly Dictionary<int, FileSystemSyncAccessHandle> _handles = [];
    private int _lastHandleId = 0;

    public FileSystemSyncAccessHandle GetHandle(int id)
    {
        if (!TryGetHandle(id, out FileSystemSyncAccessHandle? handle) || handle == null) throw new ObjectDisposedException(nameof(id));
        
        return handle;
    }

    public bool TryGetHandle(int id, [NotNullWhen(true)] out FileSystemSyncAccessHandle? handle) => _handles.TryGetValue(id, out handle);

    public int RegisterHandle(FileSystemSyncAccessHandle handle)
    {
        _handles[_lastHandleId] = handle;

        return _lastHandleId++;
    }

    public bool UnregisterHandle(int id)
    {
        if (!_handles.Remove(id, out FileSystemSyncAccessHandle? handle))
            return false;
        
        handle?.Flush();
        handle?.Close();
        handle?.Dispose();

        return true;
    }
}