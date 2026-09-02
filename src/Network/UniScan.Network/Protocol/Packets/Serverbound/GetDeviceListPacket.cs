using MessagePack;
using UniScan.Network.Protocol.Packets.Clientbound;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;
using UniScan.Network.Request;

namespace UniScan.Network.Protocol.Packets.Serverbound;

[MessagePackObject]
[RegistryPacket("UniScan", "packet", "serverbound", "get_device_list")]
[method: RequestConstructor]
public partial record GetDeviceListPacket([property: Key(0)] Guid? RequestId) : IServerboundPacket, IRequiresAcceptedClientPayloadPart<GetDeviceListPacket>, IRequiresAuthenticationPayloadPart<GetDeviceListPacket>, IRequestPayloadPart<DeviceListPacket>;