using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using UniScan.Client.App.Core.Extensions;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.SSR;
using UniScan.Client.Core.SSR;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;
using ContainerUIControl = UniScan.UserInterface.Nodes.Layout.Container.ContainerUIControl;

namespace UniScan.Client.App.Core.Module.Modules.Internal.ViewConverter;

public class ContainerUIControlViewConverter(IServiceProvider serviceProvider)
    : IUiNodeViewConverter<ContainerUIControl>
{
    public bool CanCreateView(IUINode node) => node is ContainerUIControl;
    
    public object CreateView(ContainerUIControl node, SSRSlotContext ctx)
    {
        IUIViewFactory factory = serviceProvider.GetRequiredService<IUIViewFactory>();
        
        StackPanel stackPanel = new()
        {
            Orientation = node.FlowDirection.AsAvaloniaOrientation(),
            FlowDirection = node.FlowDirection.AsAvaloniaFlowDirection(),
            Spacing = node.ItemSpacing,
            Tag = !string.IsNullOrWhiteSpace(node.Id) ? node.Id : null
        };

        foreach (IUINode n in node.Children)
        {
            stackPanel.Children.Add(new SSRNodeControl(factory, ctx, n));
        }
        
        Border b = node.Style.BuildStyledBorder();
        b.Child = stackPanel;
        b.DataContext = node;

        return b;
    }
}