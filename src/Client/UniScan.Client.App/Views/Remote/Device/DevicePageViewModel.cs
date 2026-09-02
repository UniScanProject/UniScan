using Shiki.Common.Identity;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.SSR;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote.Device;

namespace UniScan.Client.App.Views.Remote.Device;

public class DevicePageViewModel : ViewModelBase
{
    public RemoteDevice Device { get; }
    
    public UISlotControlViewModel SSRViewModel { get; }

    public DevicePageViewModel(RemoteDevice device, IUISlotRegistry registry)
    {
        Device = device;

        SSRViewModel = new UISlotControlViewModel(new Identifier("UniScan", "ssr", "slot", "device", Device.Identifier));
        registry.Add(SSRViewModel);
    }
}