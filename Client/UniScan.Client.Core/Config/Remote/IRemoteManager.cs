using ObservableCollections;
using UniScan.Client.Core.Config.Types;

namespace UniScan.Client.Core.Config.Remote;

public interface IRemoteManager
{
    ObservableList<RemoteServer> Remotes { get; }
    
    // void AddRemote(RemoteServer remote);
    // bool RemoveRemote(Types.RemoteServer remote);
    // bool RemoveRemote(Identifier id, out RemoteServer? remote);
    //
    // RemoteServer GetRemote(Identifier id);
    // bool TryGetRemote(Identifier id, [NotNullWhen(true)] out RemoteServer? remote);
}