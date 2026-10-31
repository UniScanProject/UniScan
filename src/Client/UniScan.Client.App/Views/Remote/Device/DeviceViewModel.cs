using System;
using R3;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.UI.ServersideRendering.Binding;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote.Device;
using UniScan.Client.Core.SSR;

namespace UniScan.Client.App.Views.Remote.Device;

public class DeviceViewModel : ViewModelBase, IDisposable
{
    public RemoteDevice Device { get; }
    private readonly CompositeDisposable _disposables = new();
    
    public DeviceRootPageViewModel RootPageViewModel { get; }

    public DeviceViewModel(RemoteDevice device, IUIBindingsRegistry bindingsRegistry, IUISlotRegistry slotRegistry, IRemoteRootPageDeviceNavigatorProxy remoteNavigator)
    {
        Device = device;
        RootPageViewModel = new DeviceRootPageViewModel(this, slotRegistry, remoteNavigator);
        
        bindingsRegistry.RegisterDict(Device.States).AddTo(_disposables);
    }
    
    public void Dispose()
    {
        _disposables.Dispose();
        Device.States.Clear();
    }
}