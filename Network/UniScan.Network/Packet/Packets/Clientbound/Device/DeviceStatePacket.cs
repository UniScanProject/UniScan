using MessagePack;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Core.State.Node;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "state")]
[MessagePackObject]
public record DeviceStatePacket(
    [property: Key(0)] Dictionary<Identifier, IDeviceStateNode> States,
    [property: Key(1)] Guid? RequestId,
    [property: Key(2)] Slug<SnakeSlugFormatter> ScannerIdentifier
) : IClientboundPacket, IResponsePayloadPart, ISelectedScannerPayloadPart;