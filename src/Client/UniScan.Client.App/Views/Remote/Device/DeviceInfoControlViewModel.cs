using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using R3;
using Serilog;
using Shiki.Common.Identity;
using Shiki.Internationalization.Component;
using UniScan.Client.App.Views.Global.Dialog;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Client.App.Views.Remote.Device;

public partial class DeviceInfoControlViewModel : ViewModelBase, IDisposable
{
    public DeviceViewModel DeviceViewModel { get; }
    
    public bool HasModelInfo => DeviceViewModel.Device.Specs != null;

    private readonly CompositeDisposable _subscriptions = new();
    
    private readonly ILogger _logger = Log.ForContext<DeviceInfoControlViewModel>();

    private readonly IRemoteRootPageDeviceNavigatorProxy _navigator;
    
    public DeviceInfoControlViewModel(DeviceViewModel deviceViewModel, IRemoteRootPageDeviceNavigatorProxy navigator)
    {
        DeviceViewModel = deviceViewModel;
        _navigator = navigator;

        DeviceViewModel.Device.Subscribed.AsObservable().Skip(1).Subscribe(b =>
        {
            if (b)
            {
                _logger.Information("Now subscribed to device '{Identifier}'", deviceViewModel.Device.Identifier);
            }
            else
            {
                _logger.Information("No longer subscribed to device '{Identifier}'", deviceViewModel.Device.Identifier);
            }
        }).AddTo(_subscriptions);
    }
    
    [RelayCommand]
    public async Task OnSubscribe()
    {
        var res = await DeviceViewModel.Device.Subscribe();
        if (!res.IsSuccessful)
        {
            object? result = await DialogHostAvalonia.DialogHost.Show(new DialogViewModel()
            {
                TitleText = new TranslatableTextComponent(Identifier.CreateInstance("UniScan:client/i18n/errors/device/subscribe_failed"), DeviceViewModel.Device.DisplayName ?? DeviceViewModel.Device.Identifier),
                ContentText = new AbsoluteTextComponent(res.Error!.ToString()),
            }, "MainDialogHost");
        }
    }

    [RelayCommand]
    public async Task ShowDevicePage()
    {
        _navigator.Navigate(DeviceViewModel.Device);
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }
}