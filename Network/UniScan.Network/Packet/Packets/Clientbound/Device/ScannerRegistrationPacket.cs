using Shiki.Common.Identity;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "registration")]
public record ScannerRegistrationPacket(
    string? DisplayName,
    Guid? RequestId,
    Identifier ScannerIdentifier
) : IClientboundPacket, IResponsePayloadPart, ISelectedScannerPayloadPart;