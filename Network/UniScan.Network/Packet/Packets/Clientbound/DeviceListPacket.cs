using MessagePack;
using Shiki.Common.Identity;
using UniScan.Network.Data;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound;

[MessagePackObject]
[RegistryPacket("UniScan", "packet", "clientbound", "device_list")]
public readonly record struct DeviceListPacket(
    [property: Key(0)] Dictionary<Identifier, DeviceDto> Devices,
    [property: Key(1)] Guid? RequestId
) : IClientboundPacket, IResponsePayloadPart;