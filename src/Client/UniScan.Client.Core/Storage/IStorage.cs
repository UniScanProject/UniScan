namespace UniScan.Client.Core.Storage;

public interface IStorageWriter<in T>
where T : class
{
    Task SaveAsync(T? data);
}

public interface IStorageReader<T>
    where T : class
{
    Task<T?> LoadAsync();
}

public interface IStorage<T> : IStorageReader<T>, IStorageWriter<T>
where T : class;