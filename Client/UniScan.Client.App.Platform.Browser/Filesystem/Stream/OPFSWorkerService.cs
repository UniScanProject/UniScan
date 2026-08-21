using SpawnDev.SpawnJS.JSObjects;
using Array = System.Array;

namespace UniScan.Client.App.Platform.Browser.Filesystem.Stream;

public class OPFSWorkerService(BrowserFileManager fileManager) : IOPFSWorkerService
{
    //I don't like this
    private readonly Dictionary<int, FileSystemSyncAccessHandle> _handles = [];
    private int _lastHandleId = 0;
    
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

        _handles.Add(_lastHandleId, handle);
        return _lastHandleId++;
    }
    
    public async Task<byte[]> ReadAsync(int id, long offset, int count)
    {
        if (!_handles.TryGetValue(id, out _)) throw new ObjectDisposedException(nameof(id));
        
        byte[] buffer = new byte[count];
        long read = await ReadIntoAsync(id, buffer, offset);

        if (read != count)
        {
            Array.Resize(ref buffer, (int)read);
        }
        
        return buffer;
    }
    
    private Task<long> ReadIntoAsync(int id, byte[] buffer, long offset)
    {
        if (!_handles.TryGetValue(id, out FileSystemSyncAccessHandle? handle)) throw new ObjectDisposedException(nameof(id));
        
        long read = handle.Read(buffer, new FileSystemSyncReadWriteOptions { At = offset });
        
        return Task.FromResult(read);
    }

    
    public Task<long> WriteAsync(int id, byte[] buffer, int offset)
    {
        if (!_handles.TryGetValue(id, out FileSystemSyncAccessHandle? handle)) throw new ObjectDisposedException(nameof(id));
        
        return Task.FromResult(handle.Write(buffer, new FileSystemSyncReadWriteOptions { At = offset }));
    }

    public Task FlushAsync(int id)
    {
        if (!_handles.TryGetValue(id, out FileSystemSyncAccessHandle? handle)) throw new ObjectDisposedException(nameof(id));
        
        handle.Flush();

        return Task.CompletedTask;
    }

    public Task TruncateAsync(int id, long c)
    {
        if (!_handles.TryGetValue(id, out FileSystemSyncAccessHandle? handle)) throw new ObjectDisposedException(nameof(id));
        
        handle.Truncate(c);

        return Task.CompletedTask;
    }

    public Task CloseAsync(int id)
    {
        if (_handles.Remove(id, out FileSystemSyncAccessHandle? handle))
        {
            handle.Flush();
            handle.Close();
            handle.Dispose();
        }

        return Task.CompletedTask;
    }
    
    public Task<long> GetSizeAsync(int id)
    {
        if (!_handles.TryGetValue(id, out FileSystemSyncAccessHandle? handle)) throw new ObjectDisposedException(nameof(id));   
        
        return Task.FromResult(handle.GetSize());
    }
}