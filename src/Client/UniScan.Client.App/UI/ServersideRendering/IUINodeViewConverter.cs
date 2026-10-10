using System;
using UniScan.Client.App.Views.SSR;
using UniScan.Client.Core.SSR;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;

namespace UniScan.Client.App.UI.ServersideRendering;

public interface IUINodeViewConverter
{
    public Type NodeType { get; }
    
    public bool CanCreateView(IUINode node);
    public object CreateView(IUINode node, SSRSlotContext ctx);
}

public interface IUiNodeViewConverter<in TNode> : IUINodeViewConverter
where TNode : IUINode
{
    Type IUINodeViewConverter.NodeType => typeof(TNode);
    
    object IUINodeViewConverter.CreateView(IUINode node, SSRSlotContext ctx) => CreateView((TNode)node, ctx);
    public object CreateView(TNode node, SSRSlotContext ctx);
}