using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Factory;
using UniScan.Client.App.UI;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.UserInterface;
using UniScan.UserInterface.Definitions;

namespace UniScan.Client.App.Core.Module.Modules.Internal.ViewModelFactory;

public class ContainerUIControlViewModelConverter(IServiceProvider serviceProvider)
    : IUiNodeViewModelConverter<ContainerUIControl>
{
    public bool CanCreateView(IUINode node) => node is ContainerUIControl;
    
    public object CreateView(ContainerUIControl node)
    {
        IUIViewFactory factory = serviceProvider.GetRequiredService<IUIViewFactory>();
        
        IEnumerable<Control> c = node.Children
                                     .Select(factory.CreateView)
                                     .Select(i => i as Control ?? ViewLocator.Instance.Build(i))
                                     .OfType<Control>();

        Border b = new()
        {
            Margin = new Thickness(node.Margin.X, node.Margin.Y, node.Margin.Z, node.Margin.W),
            Padding = new Thickness(node.Padding.X, node.Padding.Y, node.Padding.Z, node.Padding.W),
            
            Child = new StackPanel {
                Orientation = node.FlowDirection switch
                {
                    FlowDirection.TopToBottom => Orientation.Vertical,
                    FlowDirection.BottomToTop => Orientation.Vertical,
                    FlowDirection.LeftToRight => Orientation.Horizontal,
                    FlowDirection.RightToLeft => Orientation.Horizontal,
                    _                         => Orientation.Horizontal
                },
                FlowDirection = node.FlowDirection switch
                {
                    FlowDirection.LeftToRight => Avalonia.Media.FlowDirection.LeftToRight,
                    FlowDirection.RightToLeft => Avalonia.Media.FlowDirection.RightToLeft,
                    _                         => Avalonia.Media.FlowDirection.LeftToRight
                },
                Spacing = node.ItemSpacing,
            }
        };
        
        ((StackPanel)b.Child).Children.AddRange(c);

        return b;
    }
}