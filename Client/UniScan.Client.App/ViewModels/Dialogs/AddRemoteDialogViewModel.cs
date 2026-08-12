using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using UniScan.Client.Core.Config.Types;
using UniScan.Client.Core.DI.Factory;
using UniScan.Network.Client.Remote.Connection;

namespace UniScan.Client.App.ViewModels.Dialogs;

public partial class AddRemoteDialogViewModel : ViewModelBase
{
    private readonly IRemoteFactory _remoteFactory;
    
    [ObservableProperty]
    public partial string? DisplayName { get; set; }
    
    [ObservableProperty]
    public partial IRemoteConnectionMethod ConnectionMethod { get; set; }

    public RemoteServer CreatedRemote { get; private set; } = null!;
    
    public AddRemoteDialogViewModel(IRemoteFactory factory)
    {
        _remoteFactory = factory;
    }
    
    [RelayCommand]
    public void Confirm()
    {
        CreatedRemote = _remoteFactory.Create(DisplayName, ConnectionMethod);
        DialogHost.Close("MainDialogHost", CreatedRemote);
    }

    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close("MainDialogHost");
    }
}