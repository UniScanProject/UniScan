using CommunityToolkit.Mvvm.Input;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;

namespace UniScan.Client.App.Views.Remote.Connection;

public partial class DisconnectedRemotePageViewModel(string reason, RemoteViewModel? remoteViewModel) : ViewModelBase
{
    public string Reason { get; } = reason;
    public RemoteViewModel? RemoteViewModel { get; } = remoteViewModel;
    
    public IRelayCommand OkClicked { get; init; } = new RelayCommand(() => {});
}