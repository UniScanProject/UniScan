namespace UniScan.Client.Core.Config;

public interface IFile<TStored>
{
    Task<TStored> LoadAsync();
    
    Task SaveAsync(TStored stored);

    Task BackupAsync();
}