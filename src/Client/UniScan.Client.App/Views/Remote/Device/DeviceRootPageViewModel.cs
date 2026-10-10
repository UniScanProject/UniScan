using System;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.Global;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote.Device;
using UniScan.Client.Core.SSR;

namespace UniScan.Client.App.Views.Remote.Device;

public class DeviceRootPageViewModel : SubPagedViewModelBase, IDisposable
{
    public DeviceViewModel DeviceViewModel { get; }
    
    private DevicePageViewModel _mainPage;
    
    public DeviceRootPageViewModel(DeviceViewModel deviceViewModel, IUISlotRegistry registry) : base(new EmptyPageViewModel(), UniScanApp.Identifier.Derived("view_model", "device", deviceViewModel.Device.Identifier))
    {
        DeviceViewModel = deviceViewModel;

        _mainPage = new DevicePageViewModel(deviceViewModel, registry);
        CurrentSubpage = _mainPage;
    }

    public void Dispose()
    {
    }
}