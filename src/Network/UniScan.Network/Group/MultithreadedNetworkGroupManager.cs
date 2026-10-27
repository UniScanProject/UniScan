using DotNetty.Transport.Channels;
using Shiki.Common.Serialization.Polymorphism;

namespace UniScan.Network.Group;

[PolymorphicSerializable<INetworkGroupManager>(Id)]
public class MultithreadedNetworkGroupManager : INetworkGroupManager
{
    public const string Id = "Multithreaded";
    
    public IEventLoopGroup WorkerGroup { get; } = new MultithreadEventLoopGroup();

    public virtual async ValueTask DisposeAsync()
    {
        if (WorkerGroup != null) await WorkerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
    }
}