using Shiki.Common.Identity;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "version")]
public record VersionPacket(
    string Version,
    Identifier ScannerIdentifier,
    Guid? RequestId
) : IClientboundPacket, IResponsePayloadPart, ISelectedScannerPayloadPart;