using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using R3;
using Serilog;
using Shiki.Common.Identity;

namespace UniScan.Client.App.UI.ServersideRendering.Binding;

public class UIBindingsRegistry : IUIBindingsRegistry
{
    private readonly ConcurrentDictionary<Identifier, BindableReactiveProperty<object?>> _properties = [];
    private readonly ConcurrentDictionary<Identifier, IDisposable> _subscriptions = new();
    
    public ConcurrentDictionary<Identifier, IReadOnlyBindableReactiveProperty<object?>> Properties => new(_properties.ToDictionary(x => x.Key, IReadOnlyBindableReactiveProperty<object?> (x) => x.Value));
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisposable Register<TParent, TProperty>(Identifier id, Observable<TParent> parent,
                                                    Func<TParent, Observable<TProperty>> property)
    {
        var p = parent.Where(p => p != null)
                      .Select(property)
                      .Switch()
                      .DistinctUntilChanged()
                      .Select(x => (object?)x);

        return AddProperty(id, p);
    }

    public IDisposable Register<TProperty>(Identifier id, Observable<TProperty> property)
    {
        var p = property.DistinctUntilChanged()
                        .Select(object? (x) => x);

        return AddProperty(id, p);
    }

    private IDisposable AddProperty(Identifier id, Observable<object?> observable)
    {
        //get or create one if nonexistent yet
        var p = _properties.GetOrAdd(id, _ => new BindableReactiveProperty<object?>(null));
        if (_subscriptions
           .TryRemove(id, out IDisposable? old)) //if one already existed inside the subscriptions, we remove so we can listen to the new observable
        {
            old.Dispose();
        }

        _subscriptions[id] = observable.Subscribe(x => p.Value = x);

        Dispatcher.UIThread.Post(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Properties)));
        });

        return Disposable.Create(() => Unregister(id));
    }

    private void Unregister(Identifier id)
    {
        if (_subscriptions.TryRemove(id, out IDisposable? s))
        {
            s.Dispose();
        }

        _properties.TryRemove(id, out _);

        Dispatcher.UIThread.Post(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Properties)));
        });
    }

    public Observable<object?> GetObservable(Identifier id)
    {
        return _properties.GetOrAdd(id, _ => new BindableReactiveProperty<object?>(null)).AsObservable();
    }

    public object? GetValue(Identifier id) =>
        _properties.TryGetValue(id, out var observable) ? observable.Value : null;

    public void Dispose()
    {
        foreach (var s in _subscriptions) s.Value.Dispose();
        _subscriptions.Clear();

        foreach (var observable in _properties) observable.Value?.Dispose();
        _properties.Clear();
    }
}