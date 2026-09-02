using System.Numerics;
using MessagePack;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Extensions.MessagePack.Formatter.Identity.Slug;

namespace UniScan.UserInterface.Definitions;

public abstract class UIControl : IUIControl
{
    [Key(0), MessagePackFormatter(typeof(SlugMessagePackFormatter<DashSlugFormatter>))] public Slug<DashSlugFormatter> Id { get; init; }
    [Key(1)] public Vector4 Padding { get; set; } = Vector4.Zero;
    [Key(2)] public Vector4 Margin { get; set; } =  Vector4.Zero;
}