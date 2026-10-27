using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using R3;
using Shiki.Common.Util;
using Shiki.Internationalization;
using Shiki.Settings;
using Shiki.Settings.Option;

namespace UniScan.Client.App.UI.Conversion.Options;

public interface IOptionUIComponentProvider : IProvider<Control, IReadOnlyOption>
{
    bool SideMount { get; }
}

public class OptionDataTemplate(
    TextComponentResolverService resolverService,
    IEnumerable<IOptionUIComponentProvider> providers)
    : IDataTemplate
{
    private readonly ImmutableArray<IOptionUIComponentProvider> _providers = [.. providers];

    public Control? Build(object? param)
    {
        if (param is not IReadOnlyOption option) return null;

        IOptionUIComponentProvider? provider = _providers.FirstOrDefault(r => r.CanProvide(option));
        if (provider is null) return null;
        
        TextBlock name = new()
        {
            FontWeight = FontWeight.SemiBold
        };
        name.Bind(TextBlock.TextProperty, resolverService.Resolve(option.Name).AsObservable().ObserveOnDispatcher(Dispatcher.UIThread).AsSystemObservable());
        
        TextBlock description = new()
        {
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap
        };
        description.Bind(TextBlock.TextProperty, resolverService.Resolve(option.Description).AsObservable().ObserveOnDispatcher(Dispatcher.UIThread).AsSystemObservable());
        
        StackPanel detailsPanel = new()
        {
            Spacing = 5
        };
            
        detailsPanel.Children.Add(name);
        detailsPanel.Children.Add(description);
            
        Control control = provider.Provide(option);
        
        Border border = new()
        {
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(5)
        };

        border.Bind(Border.BackgroundProperty, new DynamicResourceExtension("SystemControlBackgroundBaseLowBrush"));
        
        if (provider.SideMount)
        {
            Grid grid = new()
            {
                ColumnDefinitions = new ColumnDefinitions("*, Auto"),
            };
            
            detailsPanel.VerticalAlignment = VerticalAlignment.Center;
            detailsPanel.Margin = new Thickness(0, 0, 20, 0);
            
            Grid.SetColumn(detailsPanel, 0);
            grid.Children.Add(detailsPanel);
            
            control.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(control, 1);
            grid.Children.Add(control);
            
            border.Child = grid;

            return border;
        }
        
        StackPanel panel = new()
        {
            Spacing = 10
        };
        
        panel.Children.Add(detailsPanel);
        panel.Children.Add(control);
        
        border.Child = panel;

        return border;
    }

    public bool Match(object? data) => data is IReadOnlyOption;
}