using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UniScan.Network.Client.Serialization;

public class IPEndPointConverter : JsonConverter<IPEndPoint>
{
    public override IPEndPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? s = reader.GetString();
        if (s != null && IPEndPoint.TryParse(s, out IPEndPoint? ep))
            return ep;
        
        throw new JsonException("Invalid IPEndPoint");
    }

    public override void Write(Utf8JsonWriter writer, IPEndPoint value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}