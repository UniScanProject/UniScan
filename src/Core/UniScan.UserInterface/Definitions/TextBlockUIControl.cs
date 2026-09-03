using System.Numerics;
using MessagePack;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;

namespace UniScan.UserInterface.Definitions;

public interface ITextBlockUIControl : IUIControl
{
    public int FontSize { get; }
}

[UINode("UniScan:ui/text_block")]
[MessagePackObject]
public class TextBlockUIControl : UIControl, ITextBlockUIControl
{
    public TextBlockUIControl(string text)
    {
        Text = text;
    }

    [Key(2)]
    public string Text { get; set; }
    
    [Key(3)]
    public int FontSize { get; set; } = 12;
}

[UINode("UniScan:ui/bound_text_block")]
[MessagePackObject]
public partial class BoundTextBlockUIControl : UIControl, ITextBlockUIControl
{
    public BoundTextBlockUIControl(Identifier propertyId)
    {
        PropertyId = propertyId;
    }

    private BoundTextBlockUIControl() : this(null!)
    {
    }

    [Key(2)]
    public Identifier PropertyId { get; private set; }

    [Key(3)]
    public int FontSize { get; set; } = 12;
}