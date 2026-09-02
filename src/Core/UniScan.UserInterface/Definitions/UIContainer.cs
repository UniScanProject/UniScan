using System.Numerics;
using MessagePack;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;

namespace UniScan.UserInterface.Definitions;

[method: SerializationConstructor]
public abstract class UIContainer(List<IUINode> children) : UIControl, IUIContainer
{
    [Key(3)]
    public List<IUINode> Children { get; protected set; } = children;
}