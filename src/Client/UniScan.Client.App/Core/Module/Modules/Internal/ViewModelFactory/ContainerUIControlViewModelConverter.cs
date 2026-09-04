using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Factory;
using UniScan.Client.App.Core.Extensions;
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
                                     .Select(i =>
                                      {
                                          if (i is Control ctrl) return ctrl;

                                          Control? view = ViewLocator.Instance.Build(i);
                                          view?.DataContext = node;

                                          return view;
                                      })
                                     .OfType<Control>();

        StackPanel stackPanel = new()
        {
            Orientation = node.FlowDirection switch
            {
                UserInterface.Definitions.FlowDirection.TopToBottom => Orientation.Vertical,
                UserInterface.Definitions.FlowDirection.BottomToTop => Orientation.Vertical,
                UserInterface.Definitions.FlowDirection.LeftToRight => Orientation.Horizontal,
                UserInterface.Definitions.FlowDirection.RightToLeft => Orientation.Horizontal,
                _                                                   => Orientation.Horizontal
            },
            FlowDirection = node.FlowDirection switch
            {
                UserInterface.Definitions.FlowDirection.LeftToRight => Avalonia.Media.FlowDirection.LeftToRight,
                UserInterface.Definitions.FlowDirection.RightToLeft => Avalonia.Media.FlowDirection.RightToLeft,
                _                                                   => Avalonia.Media.FlowDirection.LeftToRight
            },
            Spacing = node.ItemSpacing,
        };

        Border b = node.Style.BuildStyledBorder();
        b.Child = stackPanel;
        
        stackPanel.Children.AddRange(c);

        return b;
    }
}