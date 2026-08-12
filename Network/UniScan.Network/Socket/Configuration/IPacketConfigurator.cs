namespace UniScan.Network.Socket.Configuration;

public interface IPacketConfigurator
{
    void ConfigurePackets(PacketRegistry registry);
}