using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using R3;
using Shiki.Common.Identity;
using Shiki.Internationalization;
using Shiki.Internationalization.Component;

namespace UniScan.Client.App.UI.Properties;

public static class TextBlockTranslationTextComponentProperty
{
    private static TextComponentResolverService? _resolver;
    
    public static readonly AttachedProperty<string> TranslationIdProperty =
        AvaloniaProperty.RegisterAttached<object, TextBlock, string>("TranslationId");
    
    static TextBlockTranslationTextComponentProperty()
    {
        TranslationIdProperty.Changed.AddClassHandler<TextBlock, string>(OnChanged);
    }
    
    public static void SetTranslationId(TextBlock obj, string value) => obj.SetValue(TranslationIdProperty, value);
    public static string GetTranslationId(TextBlock obj) => obj.GetValue(TranslationIdProperty); 

    private static void OnChanged(TextBlock sender, AvaloniaPropertyChangedEventArgs args)
    {
        _resolver ??= UniScanApp.ServiceProvider.GetRequiredService<TextComponentResolverService>();

        if (args.NewValue is string id)
        {
            TranslatableTextComponent component = new(id);
            
            var s = _resolver.Resolve(component);
            sender.Bind(TextBlock.TextProperty, s.AsObservable().ObserveOnDispatcher(Dispatcher.UIThread)
                                                 .AsSystemObservable());
        }
    }
}