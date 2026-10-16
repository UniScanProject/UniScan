using Shiki.Common.Result;
using Shiki.FileSystem.Storage;

namespace UniScan.Core.Settings.Storage;

public interface IOptionStorage
{
    ValueTask<BooleanResult<Exception>> LoadAsync(IReadOnlyCollection<IOption> options);
    ValueTask<BooleanResult<Exception>> SaveAsync(IReadOnlyCollection<IOption> changed, IReadOnlyCollection<IOption> allOptions);
}

public class SingleOptionStorage<T>(IStorage<T, BooleanResult<Exception>> storage) : IOptionStorage
{
    public async ValueTask<BooleanResult<Exception>> LoadAsync(IReadOnlyCollection<IOption> options)
    {
        if (options.Count == 0)
            return new ArgumentException("Options is empty", nameof(options));

        if (options.Count > 1)
            return new ArgumentException("Multiple options provided for single option storage", nameof(options));
        
        IOption opt = options.First();

        try
        {
            T loaded = await storage.LoadAsync();
            opt.SetValue(loaded);
        }
        catch (Exception e)
        {
            return e;
        }

        return BooleanResult<Exception>.Successful;
    }

    public async ValueTask<BooleanResult<Exception>> SaveAsync(IReadOnlyCollection<IOption> changed,
                                                               IReadOnlyCollection<IOption> allOptions)
    {
        if (changed.Count == 0)
            return new ArgumentException("Options is empty", nameof(allOptions));

        if (changed.Count > 1)
            return new ArgumentException("Multiple options provided for single option storage", nameof(allOptions));
        
        IOption opt = changed.First();

        if (opt.GetValue() is not T d)
            return new ArgumentException("Changed value is not of correct type for this option storage");
        
        await storage.SaveAsync(d);
        return BooleanResult<Exception>.Successful;
    }
}