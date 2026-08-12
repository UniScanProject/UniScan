using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;

namespace UniScan.Network.Client.Remote.Connection;

public interface IRemoteConnectionMethod
{
    void Apply(Bootstrap bootstrap);
    Task<IChannel> ConnectAsync(Bootstrap bootstrap);
}