using System.Collections.Generic;
using System.Linq;
using UniScan.Network.Data.Info.Remote;

namespace UniScan.Client.App.ViewModels.Controls;

public class RemoteInfoViewModel(RemoteInfo info) : ViewModelBase
{
    public RemoteInfo Info { get; } = info;

    public List<RemoteLinkViewModel> Links { get; set; } =
        [.. info.Branding.Links.Select(l => new RemoteLinkViewModel(l))];
}