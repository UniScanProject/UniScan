using System.Collections.ObjectModel;
using ObservableCollections;
using UniScan.Client.Core.Config.Types;

namespace UniScan.Client.Core.Config.Remote;

public class RemoteManager : IRemoteManager
{
    public ObservableList<RemoteServer> Remotes { get; }
    
    public RemoteManager(IEnumerable<RemoteServer>? remotes = null)
    {
        this.Remotes = [.. remotes ?? []];
    }
}