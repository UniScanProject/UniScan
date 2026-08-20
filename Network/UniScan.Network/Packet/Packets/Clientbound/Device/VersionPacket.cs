using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "version")]
public record VersionPacket(
    string Version,
    Slug<SnakeSlugFormatter> ScannerIdentifier,
    Guid? RequestId
) : IClientboundPacket, IResponsePayloadPart, ISelectedScannerPayloadPart;