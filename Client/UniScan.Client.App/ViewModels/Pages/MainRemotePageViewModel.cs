using System;
using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using UniScan.Client.App.ViewModels.Controls;
using UniScan.Client.Core.Remote;

namespace UniScan.Client.App.ViewModels.Pages;

public partial class MainRemotePageViewModel : ViewModelBase, IDisposable
{
    public RemoteServer Remote { get; set; }
    
    [ObservableProperty]
    public partial RemoteInfoViewModel? RemoteInfoViewModel { get; set; }

    public DeviceListViewModel DeviceList { get; }
    
    private readonly CompositeDisposable _disposables = new();

    public MainRemotePageViewModel(RemoteServer remote, DeviceListViewModel deviceList, Observable<RemoteInfoViewModel?> remoteInfoViewModel)
    {
        Remote = remote;
        DeviceList = deviceList;
        
        remoteInfoViewModel.Subscribe(r =>
        {
            RemoteInfoViewModel = r;
        }).AddTo(_disposables);
    }
    
    public void Dispose()
    {
        DeviceList.Dispose();
        _disposables.Dispose();
    }
}