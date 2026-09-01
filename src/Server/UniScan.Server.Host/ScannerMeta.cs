using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Threading.Tasks;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Util;
using UniScan.Device.Device;
using UniScan.Server.Core.Host;

namespace UniScan.Server.Host;

public class ScannerMeta(string root, JsonSerializerOptions jsonOptions)
{
    public string GetSchema()
    {
        JsonNode schema = jsonOptions.GetJsonSchemaAsNode(typeof(ScannerHostDto));
        if (schema is JsonObject sc)
            sc.Insert(0, "$schema", "https://json-schema.org/draft/2020-12/schema");

        return schema.ToString();
    }

    public async Task WriteSchemaAsync() => await File.WriteAllTextAsync(Path.Combine(root, "scanner.schema.json"), GetSchema());

    public async Task<Dictionary<Slug<SnakeSlugFormatter>, ScannerHostDto>> LoadDtosAsync()
    {
        string p = Path.Combine(root, "scanners.json");
        
        if (!File.Exists(p))
        {
            throw new FileNotFoundException("No scanners meta found");
        }

        return JsonSerializer.Deserialize<Dictionary<Slug<SnakeSlugFormatter>, ScannerHostDto>>(await File.ReadAllTextAsync(p), jsonOptions) ?? [];
    }

    public async Task SaveAsync(ScannerHostManager hostManager)
    {
        string p = Path.Combine(root, "scanners.json");

        await using FileStream fs = new(p, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        await JsonSerializer.SerializeAsync(fs, hostManager.Scanners.ToDictionary(h => h.Key, h => new ScannerHostDto(h.Value)), jsonOptions);
    }
}