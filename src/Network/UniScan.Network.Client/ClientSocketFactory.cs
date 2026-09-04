using DotNetty.Transport.Channels;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Registry;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Network.Client;

public class ClientSocketFactory(
    PacketRegistry registry,
    IEnumerable<IPipelineConfigurator> configurators,
    IServiceProvider provider
) : IClientSocketFactory
{
    private readonly MultithreadEventLoopGroup _group = new();
    
    public ClientSocket CreateInstance(IRemoteConnectionMethod connectionMethod) => new(new UniScanClientChannelInitializer(registry, configurators, provider), connectionMethod, _group);

    public async ValueTask DisposeAsync()
    {
        await _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
    }
}