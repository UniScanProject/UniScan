using Shiki.Common.Identity;
using UniScan.Network.Data;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "info")]
public readonly record struct InfoPacket(
    Identifier ScannerIdentifier,
    DeviceDto Device,
    Guid? RequestId
) : IClientboundPacket, IResponsePayloadPart, ISelectedScannerPayloadPart;