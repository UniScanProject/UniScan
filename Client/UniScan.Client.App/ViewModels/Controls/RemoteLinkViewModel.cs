using UniScan.Network.Data.Info.Remote;

namespace UniScan.Client.App.ViewModels.Controls;

public class RemoteLinkViewModel(RemoteLink link) : ViewModelBase
{
    public RemoteLink Link { get; } = link;
}