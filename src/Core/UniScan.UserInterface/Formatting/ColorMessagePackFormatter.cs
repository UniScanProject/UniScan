using System.Drawing;
using MessagePack;
using MessagePack.Formatters;

namespace UniScan.UserInterface.Formatting;

public class ColorMessagePackFormatter : IMessagePackFormatter<Color>
{
    public void Serialize(ref MessagePackWriter writer, Color value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(4);
        writer.Write(value.R);
        writer.Write(value.G);
        writer.Write(value.B);
        writer.Write(value.A);
    }

    public Color Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
            return Color.Empty;
        
        if (!reader.TryReadArrayHeader(out int count))
            throw new MessagePackSerializationException("No array header!!!");

        if (count != 4)
            throw new MessagePackSerializationException("Array header count does not match expected '4'");

        byte r = reader.ReadByte();
        byte g = reader.ReadByte();
        byte b = reader.ReadByte();
        byte a = reader.ReadByte();

        return Color.FromArgb(a, r, g, b);
    }
}