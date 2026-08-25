using System.Text.Json;

namespace UniScan.Client.Core.Storage.Serializer;

public class JsonStorageSerializer<T>(JsonSerializerOptions? defaultOptions = null) : IStorageFileSerializer<T>
{
    public string FileExtension => "json";
    
    public async Task<T?> DeserializeAsync(Stream stream, CancellationToken ct = default) => await JsonSerializer.DeserializeAsync<T>(stream, defaultOptions, ct);
    public async Task<T?> DeserializeAsync(Stream stream, JsonSerializerOptions options, CancellationToken ct = default) => await JsonSerializer.DeserializeAsync<T>(stream, options, ct);

    public async Task SerializeAsync(Stream stream, T data, CancellationToken ct = default) => await JsonSerializer.SerializeAsync(stream, data, defaultOptions, ct);
    public async Task SerializeAsync(Stream stream, T data, JsonSerializerOptions options, CancellationToken ct = default) => await JsonSerializer.SerializeAsync(stream, data, options, ct);
}