using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using Shiki.Common.Serialization.Polymorphism;
using UniScan.Network.Group;

namespace UniScan.Modules.LibUvNetworkGroup;

[PolymorphicSerializable<IHostNetworkGroupManager>(Id)]
public class LibUvHostNetworkGroupManager : LibUvNetworkGroupManager, IHostNetworkGroupManager
{
    public const string Id = "LibUv";
    
    public IEventLoopGroup MasterGroup { get; }
    
    public LibUvHostNetworkGroupManager()
    {
        MasterGroup = _dispatcher;
    }

    public override async ValueTask DisposeAsync()
    {
        if (MasterGroup != null) await MasterGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        await base.DisposeAsync();
    }
}