using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UniScan.Network.Util;

public class IPAddressJsonConverter : JsonConverter<IPAddress>
{
    public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? s = reader.GetString();
        if (s != null && IPAddress.TryParse(s, out IPAddress? ep))
            return ep;
        
        throw new JsonException("Invalid IPAddress");
    }

    public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}