using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Device;
using UniScan.Network.Data;
using UniScan.Network.Packet.Packets.Serverbound.Subscription;

namespace UniScan.Client.App.Views.Remote.Device;

public partial class DeviceInfoControlViewModel(RemoteDevice device, RemoteServer parentRemote) : ViewModelBase
{
    public RemoteDevice Device { get; } = device;
    public RemoteServer ParentRemote { get; } = parentRemote;

    public bool HasModelInfo => Device.Specs != null;

    [RelayCommand]
    public async Task OnSubscribe()
    {
        await ParentRemote.Socket.SendRequestAsync(SubscribePacket.CreateRequest(Device.Identifier));
    }
}