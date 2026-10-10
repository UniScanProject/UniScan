using CommunityToolkit.Mvvm.Input;
using Shiki.Internationalization.Component;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;

namespace UniScan.Client.App.Views.Remote.Connection;

public partial class DisconnectedRemotePageViewModel(ITextComponent reason, RemoteViewModel? remoteViewModel) : ViewModelBase
{
    public ITextComponent Reason { get; } = reason;
    public RemoteViewModel? RemoteViewModel { get; } = remoteViewModel;
    
    public IRelayCommand OkClicked { get; init; } = new RelayCommand(() => {});
}