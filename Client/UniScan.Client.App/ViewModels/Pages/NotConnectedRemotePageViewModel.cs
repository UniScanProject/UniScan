using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using UniScan.Client.App.Core.Pipeline.Connection;
using UniScan.Client.Core.Config.Types;

namespace UniScan.Client.App.ViewModels.Pages;

public partial class NotConnectedRemotePageViewModel(IServiceProvider provider, RemoteServer remote) : ViewModelBase
{
    public RemoteServer Remote { get; set; } = remote;

    public bool HasConnectionMethod { get; set; } = remote.ConnectionMethod != null;

    public event Action<RemoteConnectionPipeline>? OnConnecting;
    public event Action<Exception>? OnConnectFailed; 
    
    [RelayCommand]
    public async Task OnConnectClicked()
    {
        RemoteConnectionPipeline pipeline = new();
        OnConnecting?.Invoke(pipeline);

        try
        {
            await pipeline.Pipeline.RunAsync(new RemoteConnectionPipeline.TaskContexts.ConnectionContext(provider,
                                                      remote));
        }
        catch (Exception e)
        {
            OnConnectFailed?.Invoke(e);
        }
    }
}