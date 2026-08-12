using UniScan.Network.Packet.Packets.Clientbound;
using UniScan.Network.Packet.Packets.Serverbound;

namespace UniScan.Network.Packet.Packets.Bidirectional;

public interface IBidirectionalPacket : IClientboundPacket, IServerboundPacket
{
    
}