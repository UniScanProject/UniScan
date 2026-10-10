using MessagePack;
using Shiki.Internationalization.Component;
using UniScan.UserInterface.Nodes.Core.Node;
using UniScan.UserInterface.Properties;

namespace UniScan.UserInterface.Nodes.Text;

public interface ITextBlockUIControl : IUIControl
{
    public int FontSize { get; }
}

[UINode("UniScan:ui/text_block")]
[MessagePackObject]
public partial class TextBlockUIControl : UIControl, ITextBlockUIControl
{
    [SerializationConstructor]
    private TextBlockUIControl() {}
    
    public TextBlockUIControl(ITextComponent text)
    {
        Text = text;
    }
    
    public TextBlockUIControl(string text)
    {
        Text = new AbsoluteTextComponent(text);
    }

    [Key(2)]
    public ITextComponent Text { get; set; }
    
    [Key(3)]
    public int FontSize { get; set; } = 12;
}