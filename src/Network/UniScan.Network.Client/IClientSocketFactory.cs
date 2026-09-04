using UniScan.Network.Client.Remote.Connection;

namespace UniScan.Network.Client;

public interface IClientSocketFactory : IAsyncDisposable
{
    ClientSocket CreateInstance(IRemoteConnectionMethod connectionMethod);
}