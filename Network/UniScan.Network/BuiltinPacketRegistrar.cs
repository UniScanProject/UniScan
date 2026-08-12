using UniScan.Network.Packet.Packets.Bidirectional.Status;
using UniScan.Network.Packet.Packets.Clientbound;
using UniScan.Network.Packet.Packets.Clientbound.Device;
using UniScan.Network.Packet.Packets.Clientbound.Remote;
using UniScan.Network.Packet.Packets.Serverbound;
using UniScan.Network.Packet.Packets.Serverbound.Client;
using UniScan.Network.Packet.Packets.Serverbound.Device;
using UniScan.Network.Packet.Packets.Serverbound.Server;

namespace UniScan.Network;

//TODO why keep this when I can instead register all packets via reflection of RegistryPacketAttribute
public class BuiltinPacketRegistrar
{
    public static void Register(PacketRegistry registry)
    {
        //Bidirectional
        registry.Register<AcknowledgePacket>();
        registry.Register<DisconnectPacket>();
        
        //Clientbound
        registry.Register<ModelPacket>();
        registry.Register<DeviceListPacket>();
        registry.Register<ServerSoftwareInfoPacket>();
        
        //Serverbound
        registry.Register<ClientSoftwareInfoPacket>();
        registry.Register<GetRemoteInfoPacket>();
        
        registry.Register<GetDeviceListPacket>();
        registry.Register<GetModelCommandPacket>();
        registry.Register<GetVersionCommandPacket>();
        registry.Register<SetVolumeCommandPacket>();
    }
}