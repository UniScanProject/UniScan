using System;
using ObservableCollections;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;

namespace UniScan.Client.App.Views.Remote.Device;

public class DeviceListControlViewModel(RemoteViewModel server, IRemoteRootPageDeviceNavigatorProxy navigator) : ViewModelBase, IDisposable
{
    public NotifyCollectionChangedSynchronizedViewList<DeviceInfoControlViewModel> DevicesView { get; } = server.Remote.Devices.CreateView(kvp => new DeviceInfoControlViewModel(kvp.Value, navigator)).ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

    public void Dispose()
    {
        DevicesView.Dispose();
    }
}