using System;
using UniScan.UserInterface;

namespace UniScan.Client.App.UI.ServersideRendering;

public interface IUINodeViewModelConverter
{
    public Type NodeType { get; }
    
    public bool CanCreateView(IUINode node);
    public object CreateView(IUINode node);
}

public interface IUiNodeViewModelConverter<in TNode> : IUINodeViewModelConverter
where TNode : IUINode
{
    Type IUINodeViewModelConverter.NodeType => typeof(TNode);
    
    object IUINodeViewModelConverter.CreateView(IUINode node) => CreateView((TNode)node);
    public object CreateView(TNode node);
}