using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Identity;
using UniScan.Client.Core.Config.Remote;
using UniScan.Client.Core.Remote;
using UniScan.Network;
using UniScan.Network.Client;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Packet.Packets.Serverbound.Client;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.Core.DI.Factory;

public interface IRemoteFactory
{
    public RemoteServer Create(Guid id, IRemoteConnectionMethod connectionMethod);
    
    public RemoteServer Create(Guid id, RemoteDto dto, RemoteCacheDto? cache);
}

public class RemoteFactory(IServiceProvider provider) : IRemoteFactory
{
    public RemoteServer Create(Guid id, IRemoteConnectionMethod connectionMethod) => new(id, connectionMethod, provider.GetRequiredService<IClientSocketFactory>());
    public RemoteServer Create(Guid id, RemoteDto dto, RemoteCacheDto? cache) => new(id, dto, cache, provider.GetRequiredService<IClientSocketFactory>());
}