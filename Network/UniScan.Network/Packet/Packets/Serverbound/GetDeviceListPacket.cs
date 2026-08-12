using MessagePack;
using UniScan.Network.Packet.Packets.Clientbound;
using UniScan.Network.Packet.PayloadPart;
using UniScan.Network.Request;

namespace UniScan.Network.Packet.Packets.Serverbound;

[MessagePackObject]
[RegistryPacket("UniScan", "packet", "serverbound", "get_device_list")]
[method: RequestConstructor]
public partial record GetDeviceListPacket([property: Key(0)] Guid? RequestId) : IServerboundPacket, IRequiresAcceptedClientPayloadPart<GetDeviceListPacket>, IRequiresAuthenticationPayloadPart<GetDeviceListPacket>, IRequestPayloadPart<DeviceListPacket>;