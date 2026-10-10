using MessagePack;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Extensions.MessagePack.Formatter.Identity.Slug;
using UniScan.UserInterface.Properties.Styling;

namespace UniScan.UserInterface.Nodes.Core.Node;

#pragma warning disable MsgPack005
[MessagePackObject]
public abstract class UIControl : IUIControl
{
    [Key(0), MessagePackFormatter(typeof(SlugMessagePackFormatter<DashSlugFormatter>))] public Slug<DashSlugFormatter> Id { get; init; }
    [Key(1)] public UINodeStyle Style { get; init; } = new();
}
#pragma warning restore MsgPack005