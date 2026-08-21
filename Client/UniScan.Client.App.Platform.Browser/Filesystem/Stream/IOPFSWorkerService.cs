namespace UniScan.Client.App.Platform.Browser.Filesystem.Stream;

public interface IOPFSWorkerService
{
    Task<int> OpenAsync(string path, FileMode mode);
    
    Task<byte[]> ReadAsync(int id, long offset, int count);
    
    Task<long> WriteAsync(int id, byte[] buffer, int offset);

    Task TruncateAsync(int id, long c);
    Task FlushAsync(int id);
    
    Task<long> GetSizeAsync(int id);
    
    Task CloseAsync(int id);
}