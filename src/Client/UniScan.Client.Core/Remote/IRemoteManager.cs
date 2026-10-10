using ObservableCollections;

namespace UniScan.Client.Core.Remote;

public interface IRemoteManager
{
    ObservableList<RemoteServer> Remotes { get; }
}