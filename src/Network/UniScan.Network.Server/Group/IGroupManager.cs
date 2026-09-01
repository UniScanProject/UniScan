using DotNetty.Transport.Channels;

namespace UniScan.Network.Server.Group;

/// <summary>
/// To be implemented, used to hold the groups
/// </summary>
public interface IGroupManager
{
    IEventLoopGroup MasterGroup { get; }
    IEventLoopGroup WorkerGroup { get; }
}