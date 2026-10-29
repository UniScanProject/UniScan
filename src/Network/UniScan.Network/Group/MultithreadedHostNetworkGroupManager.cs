using DotNetty.Transport.Channels;
using Shiki.Common.Serialization.Polymorphism;

namespace UniScan.Network.Group;

[PolymorphicSerializable<IHostNetworkGroupManager>(Id)]
public class MultithreadedHostNetworkGroupManager : MultithreadedNetworkGroupManager, IHostNetworkGroupManager
{
    public const string Id = "Multithreaded";
    
    public IEventLoopGroup MasterGroup { get; } = new MultithreadEventLoopGroup(1);

    public override async ValueTask DisposeAsync()
    {
        if (MasterGroup != null) await MasterGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        await base.DisposeAsync();
    }
}