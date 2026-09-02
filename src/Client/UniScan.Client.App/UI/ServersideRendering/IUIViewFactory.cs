using System;
using System.Collections.Generic;
using System.Linq;
using UniScan.UserInterface;

namespace UniScan.Client.App.UI.ServersideRendering;

public interface IUIViewFactory
{
    object CreateView(IUINode node);
}

public class UIViewFactory : IUIViewFactory
{
    private readonly Dictionary<Type, List<IUINodeViewModelConverter>> _converters;

    public UIViewFactory(IEnumerable<IUINodeViewModelConverter> converters)
    {
        _converters = converters.GroupBy(c => c.NodeType).ToDictionary(g => g.Key, g => g.ToList());
    }

    public object CreateView(IUINode node)
    {
        Type t = node.GetType();
        if (!_converters.TryGetValue(t, out var converters))
            throw new KeyNotFoundException("No converter registered for " + t.FullName);
        
        foreach (IUINodeViewModelConverter c in converters)
        {
            if (c.CanCreateView(node))
            {
                return c.CreateView(node);
            }
        }

        throw new InvalidOperationException("No converters could create view for " + t.FullName);
    }
}