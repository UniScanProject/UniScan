using System.Collections.Generic;
using System.Linq;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Network.Data.Info.Remote;

namespace UniScan.Client.App.Views.Remote;

public class RemoteInfoControlViewModel(RemoteInfo info) : ViewModelBase
{
    public RemoteInfo Info { get; } = info;

    public List<RemoteLinkControlViewModel> Links { get; set; } =
        [.. info.Branding.Links.Select(l => new RemoteLinkControlViewModel(l))];
}