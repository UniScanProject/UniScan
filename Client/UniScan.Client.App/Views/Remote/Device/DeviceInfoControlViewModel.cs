using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;
using UniScan.Network.Data;
using UniScan.Network.Packet.Packets.Serverbound.Subscription;

namespace UniScan.Client.App.Views.Remote.Device;

public partial class DeviceInfoControlViewModel(DeviceDto info, RemoteServer parentRemote) : ViewModelBase
{
    public DeviceDto Info { get; } = info;
    public RemoteServer ParentRemote { get; } = parentRemote;

    public bool HasModelInfo => Info.Specs != null;

    [RelayCommand]
    public async Task OnSubscribe()
    {
        await ParentRemote.Socket.SendRequestAsync(SubscribePacket.CreateRequest(Info.ScannerIdentifier));
    }
}