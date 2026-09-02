using System.Numerics;
using MessagePack;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;

namespace UniScan.UserInterface.Definitions;

public enum FlowDirection
{
    LeftToRight,
    RightToLeft,
    TopToBottom,
    BottomToTop
}

[UINode("UniScan:ui/container")]
[MessagePackObject]
public partial class ContainerUIControl(params List<IUINode> children) : UIContainer(children)
{
    public ContainerUIControl() : this([])
    {
    }
    
    [Key(4)]
    public FlowDirection FlowDirection { get; set; } = FlowDirection.TopToBottom;
    
    [Key(5)]
    public int ItemSpacing { get; set; } = 0;
}