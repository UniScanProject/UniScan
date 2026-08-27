using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using R3;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Pipeline;

namespace UniScan.Client.App.ViewModels.Pages;

public partial class NotConnectedRemotePageViewModel(IServiceProvider provider, RemoteServer remote) : ViewModelBase
{
    public RemoteServer Remote { get; set; } = remote;

    public bool HasConnectionMethod { get; set; } = remote.ConnectionMethod != null;

    public event Action<RemoteConnectionPipeline>? OnConnecting;
    public event Action<Exception>? OnConnectFailed;

    private CancellationTokenSource? _cts;
    private IDisposable? _disposable;
    
    [RelayCommand]
    public async Task OnConnectClicked()
    {
        RemoteConnectionPipeline pipeline = new();
        
        _cts = new CancellationTokenSource();
        _disposable = Remote.Connected.Skip(1).Subscribe(OnConnectionStatusChanged);
        OnConnecting?.Invoke(pipeline);

        try
        {
            await pipeline.Pipeline.RunAsync(new RemoteConnectionPipeline.TaskContexts.ConnectionContext(provider,
                                                      remote), _cts.Token);
        }
        catch (Exception e)
        {
            OnConnectFailed?.Invoke(e);
        }
    }

    private void OnConnectionStatusChanged(bool connected)
    {
        if (!connected)
        {
            _cts?.Cancel();
            _cts = null;
            
            _disposable?.Dispose();
            _disposable = null;
        }
    }
}