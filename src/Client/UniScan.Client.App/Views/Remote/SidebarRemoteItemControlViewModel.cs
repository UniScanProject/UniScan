using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;

namespace UniScan.Client.App.Views.Remote;

public partial class SidebarRemoteItemControlViewModel : ViewModelBase, IDisposable
{
    public RemoteServer Remote { get; }

    private readonly RemoteRootPageViewModel _rootPageViewModel;

    public SidebarRemoteItemControlViewModel(IServiceProvider provider, RemoteServer remote)
    {
        this.Remote = remote;

        _rootPageViewModel = new RemoteRootPageViewModel(provider, Remote);
    }

    [RelayCommand]
    public async Task OnClicked()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage(_rootPageViewModel), MainViewModel.Identifier);
    }
    
    [RelayCommand]
    public async Task OnConnectClicked()
    {
    }

    public void Dispose()
    {
        _rootPageViewModel.Dispose();
    }
}