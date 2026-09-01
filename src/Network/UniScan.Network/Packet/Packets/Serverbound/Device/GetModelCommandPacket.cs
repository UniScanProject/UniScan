using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Packet.Packets.Clientbound.Device;
using UniScan.Network.Packet.PayloadPart;
using UniScan.Network.Request;

namespace UniScan.Network.Packet.Packets.Serverbound.Device;

[RegistryPacket("UniScan", "packet", "serverbound", "device", "get_model")]
[method: RequestConstructor]
public partial record GetModelCommandPacket(Slug<SnakeSlugFormatter> ScannerIdentifier, Guid? RequestId) : IServerboundPacket, ISelectedScannerPayloadPart, IRequiresAuthenticationPayloadPart<GetModelCommandPacket>, IRequestPayloadPart<ModelPacket>;