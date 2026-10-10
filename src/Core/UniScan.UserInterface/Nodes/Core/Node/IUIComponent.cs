namespace UniScan.UserInterface.Nodes.Core.Node;

/// <summary>
/// Defines a reusable non-core component to build an IUINode from
/// </summary>
public interface IUIComponent
{
    public IUINode? Build();
}