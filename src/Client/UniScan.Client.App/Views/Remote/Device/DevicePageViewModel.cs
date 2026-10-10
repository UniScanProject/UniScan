using System;
using Shiki.Common.Identity;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.SSR;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote.Device;
using UniScan.Client.Core.SSR;
using UniScan.Network.Client.Serialization;

namespace UniScan.Client.App.Views.Remote.Device;

public class DevicePageViewModel : ViewModelBase
{
    public DeviceViewModel DeviceViewModel { get; }
    
    public SSRSlotContext SSRViewModel { get; }

    public DevicePageViewModel(DeviceViewModel deviceViewModel, IUISlotRegistry registry)
    {
        DeviceViewModel = deviceViewModel;
        
        Identifier id = new("UniScan", "ssr", "slot", "device", DeviceViewModel.Device.Identifier);
        if (registry.TryGet(id, out SSRSlotContext? vm))//stupid hack because for some reason it keeps creating new SSRViewModel or something weird
        {
            SSRViewModel = vm;
        }
        else
        {
            SSRViewModel = new SSRSlotContext(id);
            registry.Add(SSRViewModel);
        }
    }
}