using MessagePack;
using Shiki.Common.Identity;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "model")]
[MessagePackObject]
public readonly record struct ModelPacket(
    [property: Key(0)] string Model,
    [property: Key(1)] Identifier ScannerIdentifier,
    [property: Key(2)] Guid? RequestId
) : IClientboundPacket, IResponsePayloadPart, ISelectedScannerPayloadPart;