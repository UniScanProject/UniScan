using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Core.State;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "state")]
public record StatePacket(
    DeviceState State,
    Guid? RequestId,
    Slug<SnakeSlugFormatter> ScannerIdentifier
) : IClientboundPacket, IResponsePayloadPart, ISelectedScannerPayloadPart;