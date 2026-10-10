#if DEBUG
//why cant I just pass var in rider
//whatever this is fine for now
#define SSR_RETURN_DEBUG_OBJ
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using Serilog;
using Shiki.Common.Result;
using UniScan.Client.App.Views.SSR;
using UniScan.Client.Core.SSR;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;
#if DEBUG
using Avalonia.Controls;
#endif

namespace UniScan.Client.App.UI.ServersideRendering;

public class UIViewFactory : IUIViewFactory
{
    private readonly Dictionary<Type, List<IUINodeViewConverter>> _converters;
    private readonly ILogger _logger = Log.ForContext<UIViewFactory>();

    public UIViewFactory(IEnumerable<IUINodeViewConverter> converters)
    {
        _converters = converters.GroupBy(c => c.NodeType).ToDictionary(g => g.Key, g => g.ToList());
    }

    public Result<object, Exception> CreateView(IUINode node, SSRSlotContext ctx)
    {
        Type t = node.GetType();
        if (!_converters.TryGetValue(t, out var converters))
        {
            return Dispatcher.UIThread.Invoke(() => CreateError($"No converter registered for '{t.FullName}'"));
        }

        foreach (IUINodeViewConverter c in converters)
        {
            if (c.CanCreateView(node))
            {
                return Dispatcher.UIThread.Invoke(() => c.CreateView(node, ctx));
            }
        }

        return Dispatcher.UIThread.Invoke(() => CreateError($"No converters could create view for '{t.FullName}'"));
    }

    //exists for slightly cleaner handling
    //because I want that debug case but don't want to copy it around
    private Result<object, Exception> CreateError(string msg)
    {
        _logger.Error("Couldn't create an SSR object: {Message}", msg);
        
#if SSR_RETURN_DEBUG_OBJ
        return new TextBlock {
            Text = msg,
            Foreground = Avalonia.Media.Brushes.Red,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
#endif
        return new InvalidOperationException(msg);
    }
}