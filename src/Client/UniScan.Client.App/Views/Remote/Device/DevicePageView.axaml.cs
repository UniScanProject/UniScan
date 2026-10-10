using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Shiki.Common.Identity;
using UniScan.Network.Client.Serialization;
using UniScan.Network.Data.SSR;

namespace UniScan.Client.App.Views.Remote.Device;

public partial class DevicePageView : UserControl, IAsyncDisposable
{
    private UIContextSubscriptionManager.ISubscription? _ssrContextSubscription;
    
    public DevicePageView()
    {
        InitializeComponent();
        
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (_ssrContextSubscription != null)
        {
            await _ssrContextSubscription.DisposeAsync();

            if (DataContext is DevicePageViewModel vm)
            {
                vm.SSRViewModel.Node.Value = null;
                vm.SSRViewModel.NodeMap.Clear();
            }
        }
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is DevicePageViewModel vm)
        {
            Identifier ssrId = new("UniScan", "ssr", "context", "device");
            if (vm.DeviceViewModel.Device.Parent.Socket.Connected)
            {
                _ssrContextSubscription = await vm.DeviceViewModel.Device.Parent.Socket.SubscribeToUIContextAsync(ssrId, new DeviceUIContextSubscribeArgs(vm.DeviceViewModel.Device.Identifier, vm.SSRViewModel.Identifier));
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_ssrContextSubscription != null)
        {
            await _ssrContextSubscription.DisposeAsync();
        }
    }
}