using System;
using System.Collections.Generic;
using System.ComponentModel;
using R3;
using Shiki.Common.Identity;

namespace UniScan.Client.App.UI.ServersideRendering.Binding;

public interface IUIBindingsRegistry : IDisposable
{
    IDisposable Register<TParent, TProperty>(Identifier id, Observable<TParent> parent,
                                             Func<TParent, Observable<TProperty>> property);
    
    IDisposable Register<TProperty>(Identifier id, Observable<TProperty> property);
    
    Observable<object?> GetObservable(Identifier id);
    Observable<TProperty?> GetObservable<TProperty>(Identifier id) => GetObservable(id).Select(x => x is TProperty property ? property : default);
    
    object? GetValue(Identifier id);
    TProperty? GetValue<TProperty>(Identifier id) => GetValue(id) is TProperty property ? property : default;
}