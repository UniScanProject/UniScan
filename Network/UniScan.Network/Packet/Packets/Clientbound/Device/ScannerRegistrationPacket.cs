using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "registration")]
public record ScannerRegistrationPacket(
    string? DisplayName,
    Guid? RequestId,
    Slug<SnakeSlugFormatter> ScannerIdentifier
) : IClientboundPacket, IResponsePayloadPart, ISelectedScannerPayloadPart;