using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using R3;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Pipeline;

namespace UniScan.Client.App.Views.Remote.Connection;

public partial class NotConnectedRemotePageViewModel(RemoteViewModel remoteViewModel) : ViewModelBase
{
    public RemoteViewModel RemoteViewModel { get; set; } = remoteViewModel;

    public bool HasConnectionMethod { get; set; } = remoteViewModel.Remote.ConnectionMethod != null;
    
    [RelayCommand(IncludeCancelCommand = true)]
    public async Task Connect(CancellationToken ct = default)
    {
        await RemoteViewModel.ConnectAsync(ct);
    }
}