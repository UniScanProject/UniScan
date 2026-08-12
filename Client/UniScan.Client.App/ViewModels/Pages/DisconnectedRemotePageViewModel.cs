using CommunityToolkit.Mvvm.Input;

namespace UniScan.Client.App.ViewModels.Pages;

public partial class DisconnectedRemotePageViewModel(string reason) : ViewModelBase
{
    public string Reason { get; } = reason;
    public IRelayCommand OkClicked { get; init; } = new RelayCommand(() => {});
}