using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using UniScan.Client.Core.Remote;
using UniScan.Network.Data;
using UniScan.Network.Packet.Packets.Bidirectional.Status;
using UniScan.Network.Packet.Packets.Serverbound.Subscription;

namespace UniScan.Client.App.ViewModels.Controls;

public partial class DeviceInfoViewModel(DeviceDto info, RemoteServer parentRemote) : ViewModelBase
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