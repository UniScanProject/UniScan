using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using UniScan.Network.Group;

namespace UniScan.Network.Server.HostMethod;

public interface IServerHostMethod
{
    void Apply(ServerBootstrap bootstrap, IHostNetworkGroupManager networkGroupManager);
    void ApplyPipeline(IChannelPipeline pipeline);
    
    Task<IChannel> BindAsync(ServerBootstrap bootstrap);

    string ToDisplayString();
}