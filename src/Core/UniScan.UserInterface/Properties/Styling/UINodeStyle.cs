using System.Drawing;
using System.Numerics;
using MessagePack;
using UniScan.UserInterface.Formatting;

namespace UniScan.UserInterface.Properties.Styling;

[MessagePackObject]
public class UINodeStyle
{
    [Key(0)] public Vector2 Bounds { get; set; } = Vector2.NaN;    //NOTE: if any element is NaN, treated as null.
    [Key(1)] public BoundConstraints BoundConstraints { get; set; } = BoundConstraints.NaN;//NOTE: if any element is NaN, treated as null.
    [Key(2)] public Vector4 Padding { get; set; } = Vector4.Zero;
    [Key(3)] public Vector4 Margin { get; set; } =  Vector4.Zero;
    [Key(4)] public Vector4 CornerRadius { get; set; } = Vector4.Zero;
    
    [Key(5), MessagePackFormatter(typeof(ColorMessagePackFormatter))] public Color BackgroundColor { get; set; } = Color.Transparent;//TODO lets make a way to reference colors on the client with a fallback provided by the srever
    [Key(6)] public UIControlBorderStyle Border { get; set; } = new();
    [Key(7)] public UIControlPositionPreferences Position { get; set; } = new(); 
}