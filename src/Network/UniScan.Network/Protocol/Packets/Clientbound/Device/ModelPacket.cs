using MessagePack;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;

namespace UniScan.Network.Protocol.Packets.Clientbound.Device;

[RegistryPacket("UniScan", "packet", "clientbound", "device", "model")]
[MessagePackObject]
public readonly record struct ModelPacket(
    [property: Key(0)] string Model,
    [property: Key(1)] Slug<SnakeSlugFormatter> DeviceIdentifier,
    [property: Key(2)] Guid? RequestId
) : IClientboundPacket, IResponsePayloadPart, ISelectedDevicePayloadPart;