using DotNetty.Transport.Channels;

namespace UniScan.Network.Group;

public interface IHostNetworkGroupManager : INetworkGroupManager
{
    IEventLoopGroup MasterGroup { get; }
}

/// <summary>
/// To be implemented, used to hold the groups
/// </summary>
public interface INetworkGroupManager : IAsyncDisposable
{
    IEventLoopGroup WorkerGroup { get; }
}