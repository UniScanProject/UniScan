using DotNetty.Transport.Channels;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Group;
using UniScan.Network.Registry;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Network.Client;

public class ClientSocketFactory(
    PacketRegistry registry,
    IEnumerable<IPipelineConfigurator> configurators,
    IServiceProvider provider,
    INetworkGroupManager networkGroupManager
) : IClientSocketFactory
{
    public ClientSocket CreateInstance(IRemoteConnectionMethod connectionMethod) => new(new UniScanClientChannelInitializer(registry, configurators, provider), connectionMethod, networkGroupManager);
}