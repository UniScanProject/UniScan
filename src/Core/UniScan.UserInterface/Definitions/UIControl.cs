using System.Numerics;
using System.Runtime.Intrinsics;
using MessagePack;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Extensions.MessagePack.Formatter.Identity.Slug;

namespace UniScan.UserInterface.Definitions;

public enum HorizontalPosition
{
    Stretch, Left, Center, Right
}

public enum VerticalPosition
{
    Stretch, Top, Center, Bottom
}

[MessagePackObject]
public class UIControlBorderStyle
{
    [Key(0)] public uint Color { get; set; } = 0x00000000;
    [Key(1)] public Vector4 Thickness { get; set; } = Vector4.Zero;
}

[MessagePackObject]
public class UIControlPositionPreferences
{
    [Key(0)] public VerticalPosition VerticalPosition { get; set; } = VerticalPosition.Stretch;
    [Key(1)] public HorizontalPosition HorizontalPosition { get; set; } = HorizontalPosition.Stretch;
}

[MessagePackObject]
public class UINodeStyle
{
    [Key(0)] public Vector4 Padding { get; set; } = Vector4.Zero;
    [Key(1)] public Vector4 Margin { get; set; } =  Vector4.Zero;
    [Key(2)] public Vector4 CornerRadius { get; set; } = Vector4.Zero;
    
    [Key(3)] public uint BackgroundColor { get; set; } = 0x00000000;
    [Key(4)] public UIControlBorderStyle Border { get; set; } = new();
    [Key(5)] public UIControlPositionPreferences Position { get; set; } = new(); 
}

public abstract class UIControl : IUIControl
{
    [Key(0), MessagePackFormatter(typeof(SlugMessagePackFormatter<DashSlugFormatter>))] public Slug<DashSlugFormatter> Id { get; init; }
    [Key(1)] public UINodeStyle Style { get; init; } = new();
}