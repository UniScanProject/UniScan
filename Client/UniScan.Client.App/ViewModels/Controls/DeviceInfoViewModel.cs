using UniScan.Client.Core.Config.Types;
using UniScan.Network.Data;

namespace UniScan.Client.App.ViewModels.Controls;

public class DeviceInfoViewModel(DeviceDto info, RemoteServer parentRemote) : ViewModelBase
{
    public DeviceDto Info { get; } = info;
    public RemoteServer ParentRemote { get; } = parentRemote;

    public bool HasModelInfo => Info.Specs != null;
}