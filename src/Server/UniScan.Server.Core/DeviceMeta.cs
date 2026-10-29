using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;

namespace UniScan.Server.Core;

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