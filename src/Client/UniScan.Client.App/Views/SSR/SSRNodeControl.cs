using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using R3;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Client.App.UI;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.Core.SSR;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;

namespace UniScan.Client.App.Views.SSR;

public class SSRNodeControl : Decorator, IDisposable //todo right type??
{
    private IDisposable? _subscription;
    private readonly Slug<DashSlugFormatter>? _id;
    private readonly SSRSlotContext? _slotCtx;

    private readonly IUIViewFactory _factory;

    public static readonly StyledProperty<ReactiveProperty<IUINode?>> NodeProperty =
        AvaloniaProperty.Register<SSRNodeControl, ReactiveProperty<IUINode?>>(nameof(Node));

    public ReactiveProperty<IUINode?> Node
    {
        get => GetValue(NodeProperty);
        set => SetValue(NodeProperty, value);
    }

    public SSRNodeControl()
    {
        _factory = UniScanApp.ServiceProvider.GetRequiredService<IUIViewFactory>();
    }

    public SSRNodeControl(IUIViewFactory factory, SSRSlotContext slotCtx, IUINode? node)
    {
        _factory = factory;
        _slotCtx = slotCtx;
        _id = node?.Id;

        //if have id, either get existing or add to vm nodemap
        //otherwise, create new non-reactive prop
        var nodeProperty = !string.IsNullOrWhiteSpace(_id)
                               ? _slotCtx.NodeMap.GetOrAdd(_id.Value, _ => new ReactiveProperty<IUINode?>(node))
                               : new ReactiveProperty<IUINode?>(node);

        _subscription = nodeProperty.Subscribe(Render);
    }

    private void Render(IUINode? n)
    {
        Dispatcher.UIThread.Post(() =>
        {
        
        //if new node is null, we remove our child
        //because obviously it was removed
        if (n == null)
        {
            Child = null;
            return;
        }

        //create view
        var r = _factory.CreateView(n, _slotCtx!);
        if (r.HasValue) //if we have view
        {
            if (r.Value is Control ctrl) //if we can just directly assign it cuz avalonia control
            {
                Child = ctrl;
            }
            else //otherwise, locate the control to build from vm via viewlocator
            {
                Child = ViewLocator.Instance.Build(r.Value);
            }
        }
#if DEBUG
        //fallback
        else if (!r.HasValue)
        {
            Child = new TextBlock { Text = r.Error.Message };
        }
#endif
        });
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        switch (change.Property)
        {
            case var p when p == NodeProperty:
            {
                _subscription?.Dispose();

                if (change.NewValue is ReactiveProperty<IUINode?> nodeProperty)
                {
                    _subscription = nodeProperty.Subscribe(Render);
                }
            }
                break;
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);

        _subscription?.Dispose();

        if (!string.IsNullOrWhiteSpace(_id?.Value))//remove ourselves
        {
            _slotCtx?.NodeMap.TryRemove(_id.Value, out _);
        }
    }

    public void Dispose()
    {
        _subscription?.Dispose();
        if (!string.IsNullOrWhiteSpace(_id?.Value))
        {
            _slotCtx?.NodeMap.TryRemove(_id.Value, out _);
        }
    }
}