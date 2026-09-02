using MessagePack;
using MessagePack.Formatters;
using Semver;

namespace UniScan.Network.Formatter.Semver;

public class SemVersionFormatter : IMessagePackFormatter<SemVersion?>
{
    public void Serialize(ref MessagePackWriter writer, SemVersion? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        writer.Write(value.ToString());
    }

    public SemVersion? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil()) return null;
        
        string? version = reader.ReadString();
        if (version == null)
        {
            throw new
                MessagePackSerializationException($"No version string present");
        }
        
        return SemVersion.Parse(version, SemVersionStyles.Any);
    }
}