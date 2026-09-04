using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.Global;
using UniScan.Client.App.Views.Remote.Connection;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote.Device;

namespace UniScan.Client.App.Views.Remote.Device;

public class DeviceRootPageViewModel : SubPagedViewModelBase, IDisposable
{
    public RemoteDevice Device { get; }
    
    private DevicePageViewModel _mainPage;
    
    public DeviceRootPageViewModel(RemoteDevice device, IUISlotRegistry registry) : base(new EmptyPageViewModel(), UniScanApp.Identifier.Derived("view_model", "device", device.Identifier))
    {
        Device = device;

        _mainPage = new DevicePageViewModel(device, registry);
        CurrentSubpage = _mainPage;
    }

    public void Dispose()
    {
    }
}