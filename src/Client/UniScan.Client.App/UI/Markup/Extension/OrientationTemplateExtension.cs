using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.XamlIl.Runtime;
using Avalonia.Metadata;
using UniScan.Client.App.UI.Conversion;
using UniScan.Client.App.Views;

namespace UniScan.Client.App.UI.Markup.Extension;

public class OrientationTemplateExtension : MarkupExtension
{
    [Content]
    [TemplateContent]
    public IDataTemplate? Default { get; set; }
    [TemplateContent]
    public IDataTemplate? Landscape { get; set; }
    [TemplateContent]
    public IDataTemplate? Portrait { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (UniScanApp.ServiceProvider.GetService(typeof(MainView)) is not MainView s)
            return Default ?? Landscape ?? Portrait ?? AvaloniaProperty.UnsetValue;

        Binding b = new(nameof(Visual.Bounds))
        {
            Source = s,
            Converter = new FuncValueConverter<Rect, object?>(b =>
            {
                double w = b.Width;
                
                if (w is <= 0 or double.NaN)
                    return Default ?? Landscape ?? Portrait;
                
                return OrientationHelpers.IsWidescreen(w) ? Landscape : Portrait;
            })
        };
        
        return b;
    }
}

public class OrientationExtension : MarkupExtension
{
    [Content]
    public object? Default { get; set; }
    public object? Landscape { get; set; }
    public object? Portrait { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (UniScanApp.ServiceProvider.GetService(typeof(MainView)) is not MainView s)
            return Default ?? Landscape ?? Portrait ?? AvaloniaProperty.UnsetValue;

        Binding b = new(nameof(Visual.Bounds))
        {
            Source = s,
            Converter = new FuncValueConverter<Rect, object?>(b =>
            {
                double w = b.Width;
                
                if (w is <= 0 or double.NaN)
                    return Default ?? Landscape ?? Portrait ?? AvaloniaProperty.UnsetValue;
                
                return OrientationHelpers.IsWidescreen(w) ? Landscape : Portrait;
            })
        };
        
        return b;
    }
}