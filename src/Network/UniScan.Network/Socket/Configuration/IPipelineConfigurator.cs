using DotNetty.Transport.Channels;

namespace UniScan.Network.Socket.Configuration;

public interface IPipelineConfigurator
{
    int Priority { get; }
    
    void ConfigureFilters(IChannelPipeline pipeline);
    void ConfigureHandlers(IChannelPipeline pipeline);
}