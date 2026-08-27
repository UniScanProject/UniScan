using CommunityToolkit.Mvvm.Input;
using UniScan.Client.Core.Remote;

namespace UniScan.Client.App.ViewModels.Pages;

public partial class DisconnectedRemotePageViewModel(string reason, RemoteServer? remote) : ViewModelBase
{
    public string Reason { get; } = reason;
    public RemoteServer? Remote { get; } = remote;
    
    public IRelayCommand OkClicked { get; init; } = new RelayCommand(() => {});
}