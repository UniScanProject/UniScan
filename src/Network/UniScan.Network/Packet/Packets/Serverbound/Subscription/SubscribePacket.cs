using MessagePack;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Packet.Packets.Bidirectional.Status;
using UniScan.Network.Packet.PayloadPart;
using UniScan.Network.Request;
using UniScan.Network.User.Permission;

namespace UniScan.Network.Packet.Packets.Serverbound.Subscription;

/// <summary>
/// Used for clients to subscribe to a scanner, which lets them receive updates for it
/// </summary>
/// <param name="DeviceIdentifier">The scanner to subscribe to</param>
/// <param name="RequestId">The request ID (must be present, how do we enforce?)</param>
[RegistryPacket("UniScan", "packet", "serverbound", "subscription", "subscribe")]
[RequiredHandlerPermission("UniScan", "permission", "subscription", "subscribe")]
[MessagePackObject]
[method: RequestConstructor]
public partial record SubscribePacket(
    [property: Key(0)] Slug<SnakeSlugFormatter> DeviceIdentifier,
    [property: Key(1)] Guid? RequestId
) : IServerboundPacket, IRequiresAcceptedClientPayloadPart<GetDeviceListPacket>, ISelectedDevicePayloadPart, IRequiresAuthenticationPayloadPart<SubscribePacket>, IRequestPayloadPart<AcknowledgePacket>;