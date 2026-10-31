using MessagePack;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;
using UniScan.Network.Request;

namespace UniScan.Network.Protocol.Packets.Serverbound.Device;

[RegistryPacket("UniScan", "packet", "serverbound", "device", "client_unsubscribed")]
[MessagePackObject]
[method: RequestConstructor]
public partial record DeviceUnsubscribePacket(
    [property: Key(0)] Slug<SnakeSlugFormatter> DeviceIdentifier,
    [property: Key(1)] Guid? RequestId 
) : IServerboundPacket, IRequestPayloadPart<AcknowledgePacket>;