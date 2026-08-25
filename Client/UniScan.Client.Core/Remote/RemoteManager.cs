using ObservableCollections;

namespace UniScan.Client.Core.Remote;

public class RemoteManager(IEnumerable<RemoteServer>? remotes = null) : IRemoteManager
{
    public ObservableList<RemoteServer> Remotes { get; } = [.. remotes ?? []];
}