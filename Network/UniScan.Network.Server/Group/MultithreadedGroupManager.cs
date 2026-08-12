using DotNetty.Transport.Channels;

namespace UniScan.Network.Server.Group;

public class MultithreadedGroupManager : IGroupManager
{
    public IEventLoopGroup MasterGroup { get; } = new MultithreadEventLoopGroup(1);
    public IEventLoopGroup WorkerGroup { get; } = new MultithreadEventLoopGroup();
}