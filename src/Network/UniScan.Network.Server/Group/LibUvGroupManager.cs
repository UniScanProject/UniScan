using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;

namespace UniScan.Network.Server.Group;

public class LibUvGroupManager : IGroupManager
{
    private DispatcherEventLoopGroup _dispatcher;
    
    public IEventLoopGroup MasterGroup { get; }

    public IEventLoopGroup WorkerGroup { get; }
        
    public LibUvGroupManager()
    {
        _dispatcher = new DispatcherEventLoopGroup();
        
        MasterGroup = _dispatcher;
        WorkerGroup = new WorkerEventLoopGroup(_dispatcher);
    }
}