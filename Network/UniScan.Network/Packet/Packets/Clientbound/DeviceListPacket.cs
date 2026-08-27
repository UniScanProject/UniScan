using MessagePack;
using MessagePack.Formatters;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Data;
using UniScan.Network.Data.Info.Remote;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Packet.Packets.Clientbound;

[MessagePackObject]
[RegistryPacket("UniScan", "packet", "clientbound", "device_list")]
public readonly record struct DeviceListPacket(//TODO we should maybe send IDs instead and let the client request them all, or send one after another once requested.
    [property: Key(0)] Dictionary<Slug<SnakeSlugFormatter>, DeviceDto> Devices,
    [property: Key(1)] Guid? RequestId
) : IClientboundPacket, IResponsePayloadPart;