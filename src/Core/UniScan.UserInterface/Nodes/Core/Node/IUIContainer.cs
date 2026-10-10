namespace UniScan.UserInterface.Nodes.Core.Node;

public interface IUIContainer : IUIControl
{
    public List<IUINode> Children { get; }
}