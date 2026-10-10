using System.Collections.Generic;
using System.Linq;
using R3;
using Shiki.Internationalization;
using Shiki.Internationalization.Component.Resolver;
using SmartFormat;
using UniScan.Client.App.UI.ServersideRendering.Binding;
using UniScan.Network;

namespace UniScan.Client.App.Core.TextComponent.Resolver;

public class BoundTextComponentResolver(IUIBindingsRegistry registry) : ITextComponentResolver<BoundTextComponent>
{
    public int Priority => 0;

    public IReadOnlyBindableReactiveProperty<string> Provide(BoundTextComponent component, TextComponentResolverService resolverService)
    {
        var resolved = resolverService.Resolve(component.Format);

        if (component.Identifiers.Length == 0)
            return resolved;

        IEnumerable<Observable<object>> values = component.Identifiers.Select(id => registry.GetObservable(id.Identifier).Select(object (x) => x ?? id.Fallback ?? "null"));
        
        return resolved.AsObservable().CombineLatest(Observable.CombineLatest(values), (i1, i2) => Smart.Format(i1, i2) ?? i1).ToReadOnlyBindableReactiveProperty();
    }
}