using MessagePack;
using UniScan.UserInterface.Nodes.Core.Node;
using UniScan.UserInterface.Properties;
using UniScan.UserInterface.Properties.Styling;

namespace UniScan.UserInterface.Nodes.Layout.Container;

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