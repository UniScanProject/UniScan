using System;
using ObservableCollections;
using R3;
using Serilog;
using Shiki.FileSystem.Storage;
using Shiki.FileSystem.Storage.Directory.KeyValue;
using UniScan.Client.Core.Config.Remote;
using UniScan.Client.Core.DI.Factory;

namespace UniScan.Client.Core.Remote;

public interface IRemoteStorage : IDisposable
{
    Task LoadAsync();
}

public class RemoteStorage(
    IRemoteManager remoteManager,
    IRemoteFactory remoteFactory,
    DirectoryKeyValueStorage<RemoteDto> remoteStorage,
    DirectoryKeyValueStorage<RemoteCacheDto>? cacheStorage
) : IRemoteStorage
{
    private readonly CompositeDisposable _disposables = new();

    public async Task LoadAsync()
    {
        var dtos = await remoteStorage.LoadAsync();
        foreach (var err in dtos.Errors)
        {
            Log.Error(err.Exception, "Failed to load remote {Path}", err.Item);
        }
        
        var cacheResult = cacheStorage != null ? await cacheStorage.LoadAsync() : null;
        var caches = cacheResult?.Items ?? [];
        
        if (cacheResult != null)
        {
            foreach (var err in cacheResult.Errors)
            {
                Log.Error(err.Exception, "Failed to load remote cache at {Path}", err.Item);
            }
        }

        foreach (var dto in dtos.Items)
        {
            if (!Guid.TryParse(dto.Key, out Guid id))
            {
                Log.Error("Failed to load remote with ID {Id} because the ID could not be parsed into a GUID", id);
                continue;
            }
            
            caches.TryGetValue(id.ToString(), out RemoteCacheDto? cache);
            remoteManager.Remotes.Add(remoteFactory.Create(id, dto.Value, cache));
        }

        //add
        remoteManager.Remotes.ObserveAdd().Subscribe(async e =>
        {
            await SaveAsync(e.Value);
            ObserveRemote(e.Value);
        }).AddTo(_disposables);
        
        //remove
        remoteManager.Remotes.ObserveRemove().Subscribe(async e =>
        {
            await DeleteRemoteAsync(e.Value.Id);
        }).AddTo(_disposables);

        foreach (RemoteServer remote in remoteManager.Remotes)
        {
            ObserveRemote(remote);
        }
    }

    private async Task SaveAsync(RemoteServer remote)
    {
        RemoteDto dto = RemoteDto.FromRemoteServer(remote);
        await remoteStorage.SaveAsync(remote.Id.ToString(), dto);
    }
    
    private async Task SaveCacheAsync(RemoteServer remote)
    {
        if (cacheStorage == null)
            return;
        
        RemoteCacheDto dto = RemoteCacheDto.FromRemoteServer(remote);
        await cacheStorage.SaveAsync(remote.Id.ToString(), dto);
    }
    
    private void ObserveRemote(RemoteServer remote)
    {
        //TODO improve this by listening to multiple items when needed
        remote.RemoteInfo.AsObservable().Skip(1).Subscribe(async _ =>
        {
            await SaveCacheAsync(remote);
        }).AddTo(_disposables);
    }

    private async Task DeleteRemoteAsync(Guid id)
    {
        await remoteStorage.DeleteAsync(id.ToString());
        
        if (cacheStorage != null)
            await cacheStorage.DeleteAsync(id.ToString());
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}