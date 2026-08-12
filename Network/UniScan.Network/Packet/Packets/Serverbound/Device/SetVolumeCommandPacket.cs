using Shiki.Common.Identity;
using UniScan.Network.Packet.PayloadPart;
using UniScan.Network.User.Permission;

namespace UniScan.Network.Packet.Packets.Serverbound.Device;

[RegistryPacket("UniScan", "packet", "serverbound", "device", "set_volume")]
[RequiredHandlerPermission("UniScan", "permission", "device", "set_volume")]
public record SetVolumeCommandPacket(int Volume, Identifier ScannerIdentifier) : IServerboundPacket, ISelectedScannerPayloadPart, IRequiresAuthenticationPayloadPart<SetVolumeCommandPacket>;