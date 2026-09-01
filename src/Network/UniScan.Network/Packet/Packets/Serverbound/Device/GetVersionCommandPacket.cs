using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Packet.Packets.Clientbound.Device;
using UniScan.Network.Packet.PayloadPart;
using UniScan.Network.Request;
using UniScan.Network.User.Permission;

namespace UniScan.Network.Packet.Packets.Serverbound.Device;

[RegistryPacket("UniScan", "packet", "serverbound", "device", "get_version")]
[RequiredHandlerPermission("UniScan", "permission", "device", "get_version")]
[method: RequestConstructor]
public partial record GetVersionCommandPacket(Slug<SnakeSlugFormatter> DeviceIdentifier, Guid? RequestId) : IServerboundPacket, ISelectedDevicePayloadPart, IRequiresAuthenticationPayloadPart<GetVersionCommandPacket>, IRequestPayloadPart<VersionPacket>;