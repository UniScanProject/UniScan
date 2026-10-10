using Avalonia.Controls;
using Avalonia.Threading;
using R3;
using Shiki.Internationalization;
using UniScan.Client.App.Core.Extensions;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.Core.SSR;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;
using TextBlockUIControl = UniScan.UserInterface.Nodes.Text.TextBlockUIControl;

namespace UniScan.Client.App.Core.Module.Modules.Internal.ViewConverter;

public class TextBlockUIControlViewConverter(TextComponentResolverService resolver) : IUiNodeViewConverter<TextBlockUIControl>
{
    public bool CanCreateView(IUINode node) => node is TextBlockUIControl;
    
    public object CreateView(TextBlockUIControl node, SSRSlotContext ctx)
    {
        Border b = node.Style.BuildStyledBorder();
        
        TextBlock t = new()
        {
            FontSize = node.FontSize,
            Tag = !string.IsNullOrWhiteSpace(node.Id) ? node.Id : null,
        };

        t.Bind(TextBlock.TextProperty, resolver.Resolve(node.Text).AsObservable().ObserveOnDispatcher(Dispatcher.UIThread).AsSystemObservable());

        b.Child = t;
        b.DataContext = node;
        
        return b;
    }
}