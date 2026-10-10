using System.Diagnostics.CodeAnalysis;
using Shiki.Common.Identity;

namespace UniScan.Client.Core.SSR;

public interface IUISlotRegistry
{
    public SSRSlotContext Get(Identifier id);
    
    public bool TryGet(Identifier id, [NotNullWhen(true)] out SSRSlotContext? ctx);
    
    public void Add(SSRSlotContext control);
    public void Add(Identifier id, SSRSlotContext control);
    
    public void Remove(SSRSlotContext control);
    public void Remove(Identifier id, out SSRSlotContext? control);
}