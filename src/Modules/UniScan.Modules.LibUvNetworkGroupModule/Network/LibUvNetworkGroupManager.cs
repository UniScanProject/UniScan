using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using Shiki.Common.Serialization.Polymorphism;
using UniScan.Network.Group;

namespace UniScan.Modules.LibUvNetworkGroup.Network;

[PolymorphicSerializable<INetworkGroupManager>(Id)]
public class LibUvNetworkGroupManager : INetworkGroupManager
{
    public const string Id = "LibUv";
    
    protected DispatcherEventLoopGroup _dispatcher;
    
    public IEventLoopGroup WorkerGroup { get; }
        
    public LibUvNetworkGroupManager()
    {
        _dispatcher = new DispatcherEventLoopGroup();

        WorkerGroup = new WorkerEventLoopGroup(_dispatcher);
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (WorkerGroup != null) await WorkerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
    }
}