using DotNetty.Transport.Channels;
using UniScan.Network.Registry;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Network.Socket;

public abstract class UniScanChannelInitializer : ChannelInitializer<IChannel>
{
    protected readonly PacketRegistry _packetRegistry;
    protected readonly IReadOnlyList<IPipelineConfigurator> _configurators;
    protected readonly IServiceProvider _serviceProvider;
    
    public UniScanChannelInitializer(PacketRegistry packetRegistry, IEnumerable<IPipelineConfigurator> configurators, IServiceProvider serviceProvider)
    {
        _packetRegistry = packetRegistry;
        _serviceProvider = serviceProvider;
        _configurators = [.. configurators.OrderBy(c => c.Priority)];
    }
}