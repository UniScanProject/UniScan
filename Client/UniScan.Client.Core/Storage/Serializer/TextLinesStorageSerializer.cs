using System.Text;

namespace UniScan.Client.Core.Storage.Serializer;

public class TextLinesStorageSerializer : IStorageFileSerializer<IEnumerable<string>>
{
    public string FileExtension => "txt";
    
    public async Task<IEnumerable<string>?> DeserializeAsync(Stream stream, CancellationToken ct = default)
    {
        List<string> lines = [];
        
        using StreamReader reader = new(stream, Encoding.UTF8);
        while (await reader.ReadLineAsync(ct) is { } line)
        {
            lines.Add(line);
        }

        return lines;
    }

    public async Task SerializeAsync(Stream stream, IEnumerable<string> data, CancellationToken ct = default)
    {
        await using StreamWriter writer = new(stream);
        foreach (string line in data)
        {
            ct.ThrowIfCancellationRequested();
            await writer.WriteLineAsync(line.AsMemory(), ct);
        }
    }
}