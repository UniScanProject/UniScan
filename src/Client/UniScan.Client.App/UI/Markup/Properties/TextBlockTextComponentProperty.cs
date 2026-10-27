using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using R3;
using Shiki.Internationalization;
using Shiki.Internationalization.Component;

namespace UniScan.Client.App.UI.Properties;

public static class TextBlockTextComponentProperty
{
    private static TextComponentResolverService? _resolver;
    
    public static readonly AttachedProperty<ITextComponent> TextComponentProperty =
        AvaloniaProperty.RegisterAttached<object, TextBlock, ITextComponent>("TextComponent");

    static TextBlockTextComponentProperty()
    {
        TextComponentProperty.Changed.AddClassHandler<TextBlock, ITextComponent>(OnChanged);
    }
    
    public static void SetTextComponent(TextBlock obj, ITextComponent value) => obj.SetValue(TextComponentProperty, value);
    public static ITextComponent GetTextComponent(TextBlock obj) => obj.GetValue(TextComponentProperty);

    private static void OnChanged(TextBlock sender, AvaloniaPropertyChangedEventArgs args)
    {
        _resolver ??= UniScanApp.ServiceProvider.GetRequiredService<TextComponentResolverService>();

        if (args.NewValue is ITextComponent component)
        {
            var s = _resolver.Resolve(component);
            sender.Bind(TextBlock.TextProperty, s.AsObservable().ObserveOnDispatcher(Dispatcher.UIThread)
                                                 .AsSystemObservable());
        }
    }
}