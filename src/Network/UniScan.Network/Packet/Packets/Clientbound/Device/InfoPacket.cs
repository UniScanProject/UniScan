using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Data;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "info")]
public readonly record struct InfoPacket(
    Slug<SnakeSlugFormatter> DeviceIdentifier,
    DeviceDto Device,
    Guid? RequestId
) : IClientboundPacket, IResponsePayloadPart, ISelectedDevicePayloadPart;