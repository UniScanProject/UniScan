using MessagePack;
using UniScan.UserInterface.Nodes.Core.Node;

namespace UniScan.UserInterface.Nodes.Layout.Container;

[method: SerializationConstructor]
public abstract class UIContainer(List<IUINode> children) : UIControl, IUIContainer
{
    [Key(2)]
    public List<IUINode> Children { get; protected set; } = children;
}