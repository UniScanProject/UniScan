using UniScan.Network.Protocol.Packets.Clientbound;
using UniScan.Network.Protocol.Packets.Serverbound;

namespace UniScan.Network.Protocol.Packets.Bidirectional;

public interface IBidirectionalPacket : IClientboundPacket, IServerboundPacket
{
    
}