using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using UniScan.Client.Core.Config.Types;

namespace UniScan.Client.App.ViewModels.Pages;

public partial class NotConnectedRemotePageViewModel(RemoteServer remote) : ViewModelBase
{
    public RemoteServer Remote { get; set; } = remote;

    public bool HasConnectionMethod { get; set; } = remote.ConnectionMethod != null;
    
    [RelayCommand]
    public async Task OnAnonymousConnectClicked()
    {
        if (Remote.Socket.Connected)
            return;

        try
        {
            await Remote.Socket.StartAsync();

            if (Remote.Socket.Connected)
            {
                _ = Remote.RunConnectionAsync();
            }
            else
            {
                Log.Error("Failed to connect!");
            }
        }
        catch (Exception ex)
        {
            Log.Information(ex, "FUCK!");
        }
    }
}