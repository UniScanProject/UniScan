using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Shiki.Common.Result;

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

public class DnsEndPointConverter : JsonConverter<DnsEndPoint>
{
    public override DnsEndPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => Parse(reader.GetString()).ExpectDefault();

    public override void Write(Utf8JsonWriter writer, DnsEndPoint value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{value.Host}:{value.Port}");
    }

    public static Result<DnsEndPoint?, Exception> Parse(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return (DnsEndPoint?)null;
        
        int i = s.LastIndexOf(':');
        if (i == -1)
            return new ArgumentException("Invalid endpoint");
        
        string h = s[..i];
        string p = s[(i + 1)..];
        
        return !ushort.TryParse(p, out ushort port) ? new ArgumentException("Invalid port") : new DnsEndPoint(h, port);
    }
}

public class EndPointConverter : JsonConverter<EndPoint>
{
    public override EndPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s))
            return null;
        
        if (IPEndPoint.TryParse(s, out IPEndPoint? ep))
            return ep;

        return DnsEndPointConverter.Parse(s).ExpectDefault();
    }

    public override void Write(Utf8JsonWriter writer, EndPoint value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case DnsEndPoint dnsEndPoint:
                new DnsEndPointConverter().Write(writer, dnsEndPoint, options);
                break;
            case IPEndPoint ipEndPoint:
                new IPEndPointConverter().Write(writer, ipEndPoint, options);
                break;
        }
    }
}
