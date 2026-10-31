using System.Collections.Generic;
using System.Linq;
using R3;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Network.Data.Info.Remote;

namespace UniScan.Client.App.Views.Remote;

//TODO this is done in a stupid way but rushing to get this done asap
public class RemoteInfoControlViewModel(RemoteInfo info, string fallbackDisplayName) : ViewModelBase
{
    public RemoteInfo Info { get; } = info;
    public string DisplayName => !string.IsNullOrWhiteSpace(Info.DisplayName) ? Info.DisplayName : fallbackDisplayName;
    
    public List<RemoteLinkControlViewModel> Links { get; set; } =
        [.. info.Branding.Links.Where(l => l != default).Select(l => new RemoteLinkControlViewModel(l))];
}