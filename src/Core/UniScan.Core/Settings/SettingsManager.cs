using System.Collections.Immutable;
using ObservableCollections;
using R3;
using Serilog;
using Shiki.Common.Identity;
using Shiki.Common.Result;
using Shiki.FileSystem.Storage;
using UniScan.Core.Settings.Storage;

namespace UniScan.Core.Settings;

public class SettingsManagerBuilder
{
    public readonly record struct OptionMetadata(IOption Option, Identifier Storage);
    
    private readonly Dictionary<Identifier, OptionMetadata> _options = [];
    private readonly Dictionary<Identifier, SettingsCategory> _categories = [];
    private readonly Dictionary<Identifier, IOptionStorage> _storages = [];

    public BooleanResult<Exception> TryAddCategory(SettingsCategory category)
    {
        if (!_categories.TryAdd(category.Id, category))
            return new InvalidOperationException("Category already exists");
        
        return BooleanResult<Exception>.Successful;
    }

    public BooleanResult<Exception> TryAddOption(IOption option, Identifier storage)
    {
        if (!_storages.ContainsKey(storage))
        {
            return new KeyNotFoundException("Storage not found");
        }

        if (!_options.TryAdd(option.Identifier, new OptionMetadata(option, storage)))
        {
            return new InvalidOperationException("Option already exists");
        }
        
        return BooleanResult<Exception>.Successful;
    }

    public BooleanResult<Exception> TryAddStorage(Identifier id, IOptionStorage storage)
    {
        if (!_storages.TryAdd(id, storage))
            throw new InvalidOperationException("Storage already exists");
        
        return BooleanResult<Exception>.Successful;
    }

    public SettingsManager Build()
    {
        //todo
        return new SettingsManager(_options, _categories, _storages);
    }

    public async ValueTask<(SettingsManager settingsManager, IReadOnlyCollection<BooleanResult<Exception>> Errors)> BuildAndLoadAsync()//todo we can create another result type later that holds the value with errors
    {
        SettingsManager b = Build();
        var errors = await b.LoadAsync();

        return (b, errors);
    }
}

public class SettingsManager : IDisposable
{
    private readonly record struct LockableOptionStorageHolder(IOptionStorage OptionStorage, SemaphoreSlim Semaphore);
    
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    
    private readonly Dictionary<Identifier, SettingsManagerBuilder.OptionMetadata> _options = [];
    private readonly Dictionary<Identifier, SettingsCategory> _categories = [];
    private readonly Dictionary<Identifier, LockableOptionStorageHolder> _storages = [];
    
    private readonly Subject<SettingsManagerBuilder.OptionMetadata> _dirty = new();

    private bool _loadLock = false;
    
    private ILogger _logger = Log.ForContext<SettingsManager>();
    
    internal SettingsManager(Dictionary<Identifier, SettingsManagerBuilder.OptionMetadata> options, Dictionary<Identifier, SettingsCategory> categories, Dictionary<Identifier, IOptionStorage> storages)
    {
        _options = options;
        _categories = categories;
        _storages = storages.ToDictionary(kvp => kvp.Key, kvp => new LockableOptionStorageHolder(kvp.Value, new SemaphoreSlim(1,1)));
        
        foreach (SettingsManagerBuilder.OptionMetadata option in _options.Values)
        {
            option.Option.ValueChanged.Where(_ => !_loadLock).Subscribe(_ => _dirty.OnNext(option)).AddTo(_disposables);
        }
        
        _dirty.Chunk(TimeSpan.FromMilliseconds(500), TimeProvider.System)
              .Where(c => c.Length > 0)
              .Subscribe(ids =>
               {
                   var u = ids.DistinctBy(m => m.Option.Identifier).GroupBy(m => m.Storage).ToImmutableArray();
                   
                   foreach (var g in u)
                   {
                       if (_storages.TryGetValue(g.Key, out LockableOptionStorageHolder storage))
                       {
                           //TODO this is probably slow, lets fix it later.
                           var changed = g.Select(m => m.Option).ToImmutableList();
                           var all = _options.Values.Where(m => m.Storage == g.Key)
                                             .Select(m => m.Option)
                                             .ToImmutableList();

                           _ = RunObservedSave(storage, changed, all);
                       }
                   }
               }).AddTo(_disposables);
    }

    private async Task RunObservedSave(LockableOptionStorageHolder holder,
                                       IReadOnlyCollection<IOption> changed,
                                       IReadOnlyCollection<IOption> allOptions)
    {
        BooleanResult<Exception> res = await RunSave(holder, changed, allOptions);
        if (!res.IsSuccessful)
        {
            _logger.Error(res.Error, "Observed save failed!");
        }
    }
    
    private async ValueTask<BooleanResult<Exception>> RunSave(LockableOptionStorageHolder holder,
                                                         IReadOnlyCollection<IOption> changed,
                                                         IReadOnlyCollection<IOption> allOptions)
    {
        await holder.Semaphore.WaitAsync();
        
        var res = await holder.OptionStorage.SaveAsync(changed, allOptions);
        holder.Semaphore.Release();
        
        return res;
    }

    public async ValueTask<IReadOnlyCollection<BooleanResult<Exception>>> LoadAsync()
    {
        _loadLock = true;//maybe should use volatile or some lock but too lazy
        try
        {
            var t = _storages.Select(async kvp =>
            {
                LockableOptionStorageHolder holder = kvp.Value;

                var opt = _options.Values.Where(m => m.Storage == kvp.Key).Select(m => m.Option).ToImmutableList();

                await holder.Semaphore.WaitAsync();
                try
                {
                     return await holder.OptionStorage.LoadAsync(opt);
                }
                finally
                {
                    holder.Semaphore.Release();
                }
            });

            return await Task.WhenAll(t);//maybe should make ResultCollection later
        }
        finally
        {
            _loadLock = false;
        }
    }

    public void Dispose()
    {
        _dirty.Dispose();
        _disposables.Dispose();
        
        foreach (LockableOptionStorageHolder holder in _storages.Values)
        {
            holder.Semaphore.Dispose();
        }
    }
}