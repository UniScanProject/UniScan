using R3;
using UniScan.UserInterface.Nodes.Core.Node;

namespace UniScan.UserInterface.Nodes.Core.Reactive;

/// <summary>
/// Defines a reusable non-core component to build a reactive Node
/// </summary>
public interface IUIReactiveComponent : IDisposable
{
    public IReadOnlyBindableReactiveProperty<IUINode?> Node { get; }

    public void Initialize();
}