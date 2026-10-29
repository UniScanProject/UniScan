using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using UniScan.Network.Group;

namespace UniScan.Network.Client.Remote.Connection;

public interface IRemoteConnectionMethod
{
    void Apply(Bootstrap bootstrap, INetworkGroupManager networkGroupManager);

    void ApplyPipeline(IChannelPipeline pipeline);
    Task<IChannel> ConnectAsync(Bootstrap bootstrap);

    string ToDisplayString();
}