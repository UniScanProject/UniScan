using Shiki.Common.Identity;
using UniScan.Network.Packet.Packets.Clientbound.Device;
using UniScan.Network.Packet.PayloadPart;
using UniScan.Network.Request;
using UniScan.Network.User.Permission;

namespace UniScan.Network.Packet.Packets.Serverbound.Device;

[RegistryPacket("UniScan", "packet", "serverbound", "device", "get_version")]
[RequiredHandlerPermission("UniScan", "permission", "device", "get_version")]
[method: RequestConstructor]
public partial record GetVersionCommandPacket(Identifier ScannerIdentifier, Guid? RequestId) : IServerboundPacket, ISelectedScannerPayloadPart, IRequiresAuthenticationPayloadPart<GetVersionCommandPacket>, IRequestPayloadPart<VersionPacket>;