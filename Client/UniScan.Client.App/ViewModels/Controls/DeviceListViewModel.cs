using System;
using ObservableCollections;
using UniScan.Client.Core.Remote;

namespace UniScan.Client.App.ViewModels.Controls;

public class DeviceListViewModel(RemoteServer server) : ViewModelBase, IDisposable
{
    public INotifyCollectionChangedSynchronizedViewList<DeviceInfoViewModel> DevicesView { get; } = server.Devices.CreateView(kvp => new DeviceInfoViewModel(kvp.Value, server)).ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

    public void Dispose()
    {
        DevicesView.Dispose();
    }
}