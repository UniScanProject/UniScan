using UniScan.Client.App.ViewModels.Controls;
using UniScan.Client.Core.Config.Types;

namespace UniScan.Client.App.ViewModels.Pages;

public partial class MainRemotePageViewModel(RemoteServer remote, DeviceListViewModel deviceList) : ViewModelBase
{
    public RemoteServer Remote { get; set; } = remote;

    public DeviceListViewModel DeviceList { get; } = deviceList;
}