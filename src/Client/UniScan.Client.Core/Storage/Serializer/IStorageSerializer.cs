namespace UniScan.Client.Core.Storage.Serializer;

public interface IStorageFileSerializer<T> : IStorageSerializer<T>
{
    string FileExtension { get; }
    string FileExtensionGlob =>  $"*.{FileExtension}";
}

public interface IStorageSerializer<T>
{
    Task<T?> DeserializeAsync(Stream stream, CancellationToken ct = default);
    Task SerializeAsync(Stream stream, T data, CancellationToken ct = default);
}