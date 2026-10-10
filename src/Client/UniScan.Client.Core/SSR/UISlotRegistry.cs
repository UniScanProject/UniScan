using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Shiki.Common.Identity;

namespace UniScan.Client.Core.SSR;

public class UISlotRegistry : IUISlotRegistry
{
    private readonly Dictionary<Identifier, SSRSlotContext> _controls = [];

    public SSRSlotContext Get(Identifier id) => _controls[id];

    public bool TryGet(Identifier id, [NotNullWhen(true)] out SSRSlotContext? ctx) => _controls.TryGetValue(id, out ctx);

    public void Add(SSRSlotContext control) => Add(control.Identifier, control);

    public void Add(Identifier id, SSRSlotContext control) => _controls[control.Identifier] = control;
    

    public void Remove(SSRSlotContext control) => _controls.Remove(control.Identifier);

    public void Remove(Identifier id, out SSRSlotContext? control) => _controls.Remove(id, out control);
}