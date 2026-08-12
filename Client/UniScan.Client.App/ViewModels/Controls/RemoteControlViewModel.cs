using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using UniScan.Client.Core.Config.Types;

namespace UniScan.Client.App.ViewModels.Controls;

public partial class RemoteControlViewModel : ViewModelBase
{
    public RemoteServer Remote { get; }

    private readonly RemoteViewModel _viewModel;

    public RemoteControlViewModel(RemoteServer remote)
    {
        this.Remote = remote;

        _viewModel = new RemoteViewModel(Remote);
    }

    [RelayCommand]
    public async Task OnClicked()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage(_viewModel), MainViewModel.Identifier);
    }
    
    [RelayCommand]
    public async Task OnConnectClicked()
    {

    }
}