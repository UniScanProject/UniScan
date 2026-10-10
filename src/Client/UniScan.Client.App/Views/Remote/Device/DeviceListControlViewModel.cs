using System;
using ObservableCollections;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;

namespace UniScan.Client.App.Views.Remote.Device;

public class DeviceListControlViewModel(RemoteViewModel server, IRemoteRootPageDeviceNavigatorProxy navigator) : ViewModelBase, IDisposable
{
    public NotifyCollectionChangedSynchronizedViewList<DeviceInfoControlViewModel> DevicesView { get; } = server.DevicesView.CreateView(value => new DeviceInfoControlViewModel(value, navigator)).ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

    public void Dispose()
    {
        DevicesView.Dispose();
    }
}