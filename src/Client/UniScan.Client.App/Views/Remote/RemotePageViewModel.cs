using System;
using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using UniScan.Client.App.Views.Remote.Device;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;

namespace UniScan.Client.App.Views.Remote;

public partial class RemotePageViewModel : ViewModelBase, IDisposable
{
    public RemoteServer Remote { get; set; }
    
    [ObservableProperty]
    public partial RemoteInfoControlViewModel? RemoteInfoViewModel { get; set; }

    public DeviceListControlViewModel DeviceListControl { get; }
    
    private readonly CompositeDisposable _disposables = new();

    public RemotePageViewModel(RemoteServer remote, DeviceListControlViewModel deviceListControl, Observable<RemoteInfoControlViewModel?> remoteInfoViewModel)
    {
        Remote = remote;
        DeviceListControl = deviceListControl;
        
        remoteInfoViewModel.Subscribe(r =>
        {
            RemoteInfoViewModel = r;
        }).AddTo(_disposables);
    }
    
    public void Dispose()
    {
        DeviceListControl.Dispose();
        _disposables.Dispose();
    }
}