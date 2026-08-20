using UniScan.Platform.Filesystem;

namespace UniScan.Platform.Implementations.Native.Filesystem;

public class NativeFileManager : IPlatformFileManager
{
    public Task<bool> ExistsAsync(string path) => Task.FromResult(File.Exists(path));

    public Task CopyAsync(string from, string to, bool overwrite)
    {
        File.Copy(from, to, overwrite);
        
        return Task.CompletedTask;
    }

    public Task MoveAsync(string from, string to, bool overwrite)
    {
        File.Move(from, to, overwrite);
        
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string path)
    {
        File.Delete(path);
        
        return Task.CompletedTask;
    }

    public Task<Stream> GetStreamAsync(string path, FileMode mode, FileAccess access, FileShare share) => Task.FromResult<Stream>(File.Open(path, mode, access, share));
}