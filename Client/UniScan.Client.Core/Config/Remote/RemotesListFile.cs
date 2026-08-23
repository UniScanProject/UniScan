using System.Text.Json;
using Serilog;
using Shiki.Common.Identity;
using UniScan.Client.Core.DI.Factory;
using UniScan.Platform.Filesystem;

namespace UniScan.Client.Core.Config.Remote;

public class RemotesListFile(string root, IPlatformFileManager fileManager, IRemoteFactory factory, JsonSerializerOptions options) : IFile<IRemoteManager>
{
    private readonly string _root = root;
    private readonly string _path = Path.Combine(root, "remotes.json");
    private readonly IRemoteFactory _remoteFactory = factory;
    private readonly JsonSerializerOptions _jsonOptions = options;
    
    private readonly SemaphoreSlim _lock = new(1, 1); //my name is slim semaphore
    
    private ILogger _logger = Log.ForContext<RemotesListFile>();

    public async Task<IRemoteManager> LoadAsync()
    {
        _logger.Debug("checking for remotes list");
        if (!await fileManager.ExistsAsync(_path))
        {
            _logger.Information("Creating remotes list");
            return new RemoteManager();
        }
        
        _logger.Debug("loading remotes list");
        var r = new List<RemoteDto>();
        try
        {
            await using Stream stream =
                await fileManager.GetStreamAsync(_path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);

            var deserialize = await JsonSerializer.DeserializeAsync<List<RemoteDto>>(stream, _jsonOptions);
            if (deserialize != null)
                r = deserialize;

            await stream.FlushAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load remotes list! Will backup and create new file.");

            await BackupAsync();
            return await SaveNewAsync();
        }
        
        return new RemoteManager(r.Select(d => _remoteFactory.Create(d.ConnectionMethod)));
    }

    public async Task SaveAsync(IRemoteManager stored)
    {
        ArgumentNullException.ThrowIfNull(stored);
        
        await _lock.WaitAsync();

        try
        {
            await using Stream stream =
                await fileManager.GetStreamAsync(_path, FileMode.Create, FileAccess.Write, FileShare.None);

            Log.Information("{s}", stream);

            await JsonSerializer.SerializeAsync(stream, stored.Remotes.Select(RemoteDto.FromRemoteServer),
                                                _jsonOptions);

            await stream.FlushAsync();

            _logger.Information("Saved remotes to {Path}", _path);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save remotes to {Path}", _path);
        }
        finally
        {
            _lock.Release();
        }
    }
    
    public async Task BackupAsync()
    {
        if (!await fileManager.ExistsAsync(_path))
        {
            return;
        }

        try
        {
            await fileManager.CopyAsync(_path, GetBackupPath(), true);
            _logger.Information("[BK] {Path} => {BackupPath}", _path, GetBackupPath());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to backup remotes to {Path}", _path);
            throw ex;
        }
    }

    private string GetBackupPath() => Path.Combine(_root, $"remotes.{DateTime.Now:yyyyMMdd_HHmmss}.json.bak");
    
    private async Task<RemoteManager> SaveNewAsync()
    {
        RemoteManager rm = new();
        await SaveAsync(rm);
        
        return rm;
    }
}