namespace UniScan.Client.App.Platform.Browser.Filesystem.Stream;

public interface IOPFSWorkerService
{
    Task<string> OpenAsync(string path, FileMode mode);
    
    Task<byte[]> ReadAsync(string id, long offset, int count);
    
    Task<long> WriteAsync(string id, byte[] buffer, int offset);

    Task TruncateAsync(string id, long c);
    Task FlushAsync(string id);
    
    Task<long> GetSizeAsync(string id);
    
    Task CloseAsync(string id);
}