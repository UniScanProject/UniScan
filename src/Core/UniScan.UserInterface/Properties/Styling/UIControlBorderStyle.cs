using System.Drawing;
using System.Numerics;
using MessagePack;
using UniScan.UserInterface.Formatting;

namespace UniScan.UserInterface.Properties.Styling;

[MessagePackObject]
public class UIControlBorderStyle
{
    [Key(0), MessagePackFormatter(typeof(ColorMessagePackFormatter))] public Color Color { get; set; } = Color.Transparent;
    [Key(1)] public Vector4 Thickness { get; set; } = Vector4.Zero;
}