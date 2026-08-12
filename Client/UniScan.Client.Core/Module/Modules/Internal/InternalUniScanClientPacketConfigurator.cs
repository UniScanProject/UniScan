using UniScan.Network;
using UniScan.Network.Packet.Packets.Clientbound;
using UniScan.Network.Packet.Packets.Clientbound.Device;
using UniScan.Network.Packet.Packets.Serverbound;
using UniScan.Network.Packet.Packets.Serverbound.Device;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.Core.Module.Modules.Internal;

public class InternalUniScanClientPacketConfigurator : IPacketConfigurator
{
    public void ConfigurePackets(PacketRegistry registry)
    {
        BuiltinPacketRegistrar.Register(registry);
    }
}