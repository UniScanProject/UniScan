using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Protocol.Packets.Clientbound.Device;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;
using UniScan.Network.Request;

namespace UniScan.Network.Protocol.Packets.Serverbound.Device;

[RegistryPacket("UniScan", "packet", "serverbound", "device", "get_model")]
[method: RequestConstructor]
public partial record GetModelCommandPacket(Slug<SnakeSlugFormatter> DeviceIdentifier, Guid? RequestId) : IServerboundPacket, ISelectedDevicePayloadPart, IRequiresAuthenticationPayloadPart<GetModelCommandPacket>, IRequestPayloadPart<ModelPacket>;