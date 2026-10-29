using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using UniScan.Network.Group;
using UniScan.Network.Socket;

namespace UniScan.Audio.Server;

public interface IAudioHostMethod
{
    Task<IChannel> BindAsync(IHostNetworkGroupManager networkGroupManager, UniScanChannelInitializer channelInitializer);

    string ToDisplayString();
}