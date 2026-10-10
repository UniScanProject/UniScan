using MessagePack;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.UserInterface.Formatting;
using UniScan.UserInterface.Properties.Styling;

namespace UniScan.UserInterface.Nodes.Core.Node;

[MessagePackFormatter(typeof(UINodeMessagePackFormatter))]
public interface IUINode : IUIDefinition
{
    public Slug<DashSlugFormatter> Id { get; }

    public UINodeStyle Style { get; }
}