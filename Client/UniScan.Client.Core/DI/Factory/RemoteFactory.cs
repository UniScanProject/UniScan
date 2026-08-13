using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Identity;
using UniScan.Client.Core.Config.Types;
using UniScan.Network;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Packet.Packets.Serverbound.Client;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.Core.DI.Factory;

public interface IRemoteFactory
{
    public RemoteServer Create(string displayName, IRemoteConnectionMethod connectionMethod);
}

public class RemoteFactory(IServiceProvider provider) : IRemoteFactory
{
    public RemoteServer Create(string displayName, IRemoteConnectionMethod connectionMethod) => new(displayName, connectionMethod, provider.GetServices<IPipelineConfigurator>(), provider.GetRequiredService<PacketRegistry>(), provider);
}