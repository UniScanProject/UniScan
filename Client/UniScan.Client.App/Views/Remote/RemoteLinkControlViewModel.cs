using UniScan.Client.App.Views.ViewModel;
using UniScan.Network.Data.Info.Remote;

namespace UniScan.Client.App.Views.Remote;

public class RemoteLinkControlViewModel(RemoteLink link) : ViewModelBase
{
    public RemoteLink Link { get; } = link;
}