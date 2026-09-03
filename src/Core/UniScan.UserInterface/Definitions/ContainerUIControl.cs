using MessagePack;

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
    
    [Key(3)]
    public FlowDirection FlowDirection { get; set; } = FlowDirection.TopToBottom;
    
    [Key(4)]
    public int ItemSpacing { get; set; } = 0;
}