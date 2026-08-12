using Shiki.Common.Identity;
using UniScan.Core.State;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "state")]
public record StatePacket(
    DeviceState State,
    Guid? RequestId,
    Identifier ScannerIdentifier
) : IClientboundPacket, IResponsePayloadPart, ISelectedScannerPayloadPart;