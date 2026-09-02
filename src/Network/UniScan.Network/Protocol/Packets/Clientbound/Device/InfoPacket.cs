using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Data.Device;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;

namespace UniScan.Network.Protocol.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "info")]
public readonly record struct InfoPacket(
    Slug<SnakeSlugFormatter> DeviceIdentifier,
    DeviceDto Device,
    Guid? RequestId
) : IClientboundPacket, IResponsePayloadPart, ISelectedDevicePayloadPart;