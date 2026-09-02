using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Protocol.Packets.Clientbound.Device;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;
using UniScan.Network.Request;
using UniScan.Network.User.Permission;

namespace UniScan.Network.Protocol.Packets.Serverbound.Device;

[RegistryPacket("UniScan", "packet", "serverbound", "device", "get_version")]
[RequiredHandlerPermission("UniScan", "permission", "device", "get_version")]
[method: RequestConstructor]
public partial record GetVersionCommandPacket(Slug<SnakeSlugFormatter> DeviceIdentifier, Guid? RequestId) : IServerboundPacket, ISelectedDevicePayloadPart, IRequiresAuthenticationPayloadPart<GetVersionCommandPacket>, IRequestPayloadPart<VersionPacket>;