using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using R3;
using Serilog;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.Remote.Device;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Pipeline;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;

namespace UniScan.Client.App.Views.Remote;

public partial class RemoteViewModel : ViewModelBase, IDisposable
{
    public RemoteServer Remote { get; }
    public RemoteConnectionPipeline Pipeline { get; }
    
    private readonly IServiceProvider _serviceProvider;
    
    public DeviceListControlViewModel? DeviceListControl { get; private set; }
    public RemoteRootPageViewModel RootPageViewModel { get; }
    
    public BehaviorSubject<RemoteInfoControlViewModel?> InfoViewModelStream { get; } = new(null); 
    public RemoteInfoControlViewModel? InfoViewModel => InfoViewModelStream.Value;

    public INotifyCollectionChangedSynchronizedViewList<DeviceRootPageViewModel> DevicePages { get; }
    
    private readonly CompositeDisposable _disposables = new();
    
    private CancellationTokenSource? _connectionCts;
    
    public RemoteViewModel(RemoteServer remote, IServiceProvider provider)
    {
        Remote = remote;
        Pipeline = new RemoteConnectionPipeline();
        _serviceProvider = provider;

        RootPageViewModel = new RemoteRootPageViewModel(this);
        
        DevicePages = Remote.Devices.CreateView(kvp => new DeviceRootPageViewModel(kvp.Value, provider.GetRequiredService<IUISlotRegistry>())).ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

        Remote.RemoteInfo.Subscribe((v) =>
        {
            RemoteInfoControlViewModel? n = v != null ? new RemoteInfoControlViewModel(v) : null;
            
            InfoViewModelStream.OnNext(n);
            OnPropertyChanged(nameof(InfoViewModel));
        });
        
        Remote.ConnectionStatus.AsObservable().Skip(1).Subscribe((b) =>
        {
            if (b.State >= ConnectionState.Disconnected)
            {
                TearDown();
            }
        }).AddTo(_disposables);
    }

    [RelayCommand]
    public async Task SwitchToPage()
    {
        WeakReferenceMessenger.Default.Send(new NavigationMessage(RootPageViewModel), MainViewModel.Identifier);
    }
    
    [RelayCommand(IncludeCancelCommand = true)]
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        _connectionCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        try
        {
            DeviceListControl = new DeviceListControlViewModel(this, RootPageViewModel);
            await
                Pipeline.Pipeline.RunAsync(new RemoteConnectionPipeline.TaskContexts.ConnectionContext(_serviceProvider,
                                               Remote), _connectionCts.Token);
        }
        catch (OperationCanceledException)
        {
            
        }
        catch (Exception e)
        {
            ((IRemoteServerMutationProxy)Remote).SetConnectionStatus(new UnexpectedDisconnectedConnectionStatusContext(e));
            _connectionCts.Cancel();
        }
    }
    
    [RelayCommand]
    public async Task DisconnectAsync()
    {
        if (Remote.ConnectionStatus.Value.State >= ConnectionState.Disconnected)
        {
            return;
        }
        
        ((IRemoteServerMutationProxy)Remote).SetConnectionStatus(new DefaultConnectionStatusContext(ConnectionState.UserDisconnected));
        
        _connectionCts?.Cancel();
        
        await Remote.Socket.SendPacketAsync(new DisconnectPacket("User initiated disconnect"));
        await Task.Delay(500);
        await Remote.Socket.StopAsync();
    }

    private void TearDown()
    {
        DeviceListControl?.Dispose();
        DeviceListControl = null;
    }

    public void Dispose()
    {
        _disposables.Dispose();
        
        DevicePages.Dispose();
        DeviceListControl?.Dispose();
        
        InfoViewModelStream.Dispose();
        
        RootPageViewModel.Dispose();
    }
}