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

public static class JsonSchemaUtil
{
    public static string GetSchema<T>(JsonSerializerOptions? options = null) => GetSchema(typeof(T), options);
    
    public static string GetSchema(Type t, JsonSerializerOptions? options = null)
    {
        JsonNode schema = (options ?? JsonSerializerOptions.Default).GetJsonSchemaAsNode(t);
        if (schema is JsonObject sc)
            sc.Insert(0, "$schema", "https://json-schema.org/draft/2020-12/schema");

        return schema.ToString();
    }
}