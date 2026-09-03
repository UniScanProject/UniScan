using System.Numerics;
using MessagePack;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.UserInterface.Definitions;
using UniScan.UserInterface.Formatting;

namespace UniScan.UserInterface;

public interface IUIDefinition;

//welcome back LCE
[MessagePackFormatter(typeof(UINodeMessagePackFormatter))]
public interface IUINode : IUIDefinition
{
    public Slug<DashSlugFormatter> Id { get; }

    public UINodeStyle Style { get; }
}

public interface IUIControl : IUINode;

public interface IUIContainer : IUIControl
{
    public List<IUINode> Children { get; }
}

