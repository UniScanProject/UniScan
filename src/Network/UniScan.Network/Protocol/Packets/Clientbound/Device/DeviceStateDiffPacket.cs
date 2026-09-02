using MessagePack;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Core.State.Node;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;

namespace UniScan.Network.Protocol.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "state_diff")]
[MessagePackObject]
public record DeviceStateDiffPacket(
    [property: Key(0)] Dictionary<Identifier, IDeviceStateNode> States,
    [property: Key(1)] Slug<SnakeSlugFormatter> DeviceIdentifier
) : IClientboundPacket, ISelectedDevicePayloadPart;