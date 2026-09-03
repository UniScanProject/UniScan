using Shiki.Common.Identity;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.SSR;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote.Device;

namespace UniScan.Client.App.Views.Remote.Device;

public class DevicePageViewModel : ViewModelBase
{
    public RemoteDevice Device { get; }
    
    public IUISlotControlViewModel SSRViewModel { get; }

    public DevicePageViewModel(RemoteDevice device, IUISlotRegistry registry)
    {
        Device = device;

        Identifier id = new("UniScan", "ssr", "slot", "device", Device.Identifier);
        if (registry.TryGet(id, out IUISlotControlViewModel? vm))//stupid hack because for some reason it keeps creating new SSRViewModel or something weird
        {
            SSRViewModel = vm;
        }
        else
        {
            SSRViewModel = new UISlotControlViewModel(id);
            registry.Add(SSRViewModel);
        }
        
    }
}