using Shiki.Common.Identity;
using UniScan.Network.Packet.Packets.Clientbound.Device;
using UniScan.Network.Packet.PayloadPart;
using UniScan.Network.Request;

namespace UniScan.Network.Packet.Packets.Serverbound.Device;

[RegistryPacket("UniScan", "packet", "serverbound", "device", "get_model")]
[method: RequestConstructor]
public partial record GetModelCommandPacket(Identifier ScannerIdentifier, Guid? RequestId) : IServerboundPacket, ISelectedScannerPayloadPart, IRequiresAuthenticationPayloadPart<GetModelCommandPacket>, IRequestPayloadPart<ModelPacket>;