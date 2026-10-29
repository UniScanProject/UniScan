using UniScan.Network.Client.Remote.Connection;

namespace UniScan.Network.Client;

public interface IClientSocketFactory
{
    ClientSocket CreateInstance(IRemoteConnectionMethod connectionMethod);
}