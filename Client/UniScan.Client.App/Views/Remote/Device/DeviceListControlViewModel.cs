using System;
using ObservableCollections;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;

namespace UniScan.Client.App.Views.Remote.Device;

public class DeviceListControlViewModel(RemoteServer server) : ViewModelBase, IDisposable
{
    public INotifyCollectionChangedSynchronizedViewList<DeviceInfoControlViewModel> DevicesView { get; } = server.Devices.CreateView(kvp => new DeviceInfoControlViewModel(kvp.Value, server)).ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

    public void Dispose()
    {
        DevicesView.Dispose();
    }
}