using MessagePack;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Data.Device;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;

namespace UniScan.Network.Protocol.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "info")]
[MessagePackObject]
public readonly record struct DeviceInfoPacket(
    [property: Key(0)] Slug<SnakeSlugFormatter> DeviceIdentifier,
    [property: Key(1)] DeviceDto Device,
    [property: Key(2)] Guid? RequestId
) : IClientboundPacket, IResponsePayloadPart, ISelectedDevicePayloadPart;