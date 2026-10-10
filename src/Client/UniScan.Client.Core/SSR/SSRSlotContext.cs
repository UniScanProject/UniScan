using System.Collections.Concurrent;
using R3;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;

namespace UniScan.Client.Core.SSR;

public record SSRSlotContext(Identifier Identifier)
{
    public ReactiveProperty<IUINode?> Node { get; } = new();
    public ConcurrentDictionary<Slug<DashSlugFormatter>, ReactiveProperty<IUINode?>> NodeMap { get; } = [];
}