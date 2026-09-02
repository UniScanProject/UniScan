using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R3;
using Serilog;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Device;
using UniScan.Network.Data;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.Packets.Serverbound.Subscription;

namespace UniScan.Client.App.Views.Remote.Device;

public partial class DeviceInfoControlViewModel : ViewModelBase, IDisposable
{
    public RemoteDevice Device { get; }
    
    public bool HasModelInfo => Device.Specs != null;

    private readonly CompositeDisposable _subscriptions = new();
    
    private readonly ILogger _logger = Log.ForContext<DeviceInfoControlViewModel>();

    private readonly IRemoteRootPageDeviceNavigatorProxy _navigator;
    
    public DeviceInfoControlViewModel(RemoteDevice device, IRemoteRootPageDeviceNavigatorProxy navigator)
    {
        Device = device;
        _navigator = navigator;

        Device.Subscribed.AsObservable().Skip(1).Subscribe(b =>
        {
            if (b)
            {
                _logger.Information("Now subscribed to device '{Identifier}'", device.Identifier);
            }
            else
            {
                _logger.Information("No longer subscribed to device '{Identifier}'", device.Identifier);
            }
        }).AddTo(_subscriptions);
    }
    
    [RelayCommand]
    public async Task OnSubscribe()
    {
        await Device.Subscribe();
    }

    [RelayCommand]
    public async Task ShowDevicePage()
    {
        _navigator.Navigate(Device);
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }
}