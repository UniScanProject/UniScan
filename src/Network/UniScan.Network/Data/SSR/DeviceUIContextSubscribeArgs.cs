using MessagePack;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;

namespace UniScan.Network.Data.SSR;

[UISubscribeArgs("UniScan:ssr/args/device")]
[MessagePackObject]
public record DeviceUIContextSubscribeArgs(
    [property: Key(0)] Slug<SnakeSlugFormatter> DeviceIdentifier,
    [property: Key(1)] Identifier SlotIdentifier
) : IUISubscribeArgs;