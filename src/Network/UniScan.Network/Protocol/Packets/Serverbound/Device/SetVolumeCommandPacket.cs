using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;
using UniScan.Network.User.Permission;

namespace UniScan.Network.Protocol.Packets.Serverbound.Device;

[RegistryPacket("UniScan", "packet", "serverbound", "device", "set_volume")]
[RequiredHandlerPermission("UniScan", "permission", "device", "set_volume")]
public record SetVolumeCommandPacket(int Volume, Slug<SnakeSlugFormatter> DeviceIdentifier) : IServerboundPacket, ISelectedDevicePayloadPart, IRequiresAuthenticationPayloadPart<SetVolumeCommandPacket>;