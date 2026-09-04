using System;
using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using UniScan.Client.App.Views.Remote.Device;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Client.App.Views.Remote;

public partial class RemotePageViewModel : ViewModelBase, IDisposable
{
    public RemoteViewModel RemoteViewModel { get; }
    
    [ObservableProperty]
    public partial RemoteInfoControlViewModel? RemoteInfoViewModel { get; set; }

    
    private readonly CompositeDisposable _disposables = new();

    public RemotePageViewModel(RemoteViewModel remoteViewModel)
    {
        RemoteViewModel = remoteViewModel;
        
        RemoteViewModel.InfoViewModelStream.AsObservable().Subscribe(r =>
        {
            RemoteInfoViewModel = r;
        }).AddTo(_disposables);
    }
    
    public void Dispose()
    {
        _disposables.Dispose();
    }
}