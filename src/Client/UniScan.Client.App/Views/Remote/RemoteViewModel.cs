using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using R3;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Internationalization;
using Shiki.Internationalization.Component;
using UniScan.Client.App.Core.Helpers;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.UI.ServersideRendering.Binding;
using UniScan.Client.App.Views.Remote.Device;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Connection;
using UniScan.Client.Core.Remote.Connection.Status;
using UniScan.Client.Core.Remote.Device;
using UniScan.Client.Core.Remote.Pipeline;
using UniScan.Client.Core.SSR;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;

namespace UniScan.Client.App.Views.Remote;

//TODo I don't like calling this a viewmodel but ig that is what it is
public partial class RemoteViewModel : ViewModelBase, IDisposable
{
    public RemoteServer Remote { get; }
    public RemoteConnectionPipeline Pipeline { get; }
    
    private readonly IServiceProvider _serviceProvider;
    
    public DeviceListControlViewModel? DeviceListControl { get; private set; }
    public RemoteRootPageViewModel RootPageViewModel { get; }
    
    public BehaviorSubject<RemoteInfoControlViewModel?> InfoViewModelStream { get; } = new(null); 
    public RemoteInfoControlViewModel? InfoViewModel => InfoViewModelStream.Value;

    private readonly SynchronizedViewMirror<KeyValuePair<Slug<SnakeSlugFormatter>, RemoteDevice>, DeviceViewModel> _deviceList;
    public IReadOnlyObservableList<DeviceViewModel> DevicesView { get; }
    
    private readonly CompositeDisposable _disposables = new();
    
    private CancellationTokenSource? _connectionCts;
    
    public RemoteViewModel(RemoteServer remote, IServiceProvider provider)
    {
        Remote = remote;
        Pipeline = new RemoteConnectionPipeline();
        _serviceProvider = provider;

        RootPageViewModel = new RemoteRootPageViewModel(this, provider.GetRequiredService<TextComponentResolverService>());

        IUIBindingsRegistry bindingsRegistry = provider.GetRequiredService<IUIBindingsRegistry>();
        IUISlotRegistry slotRegistry = provider.GetRequiredService<IUISlotRegistry>();
        
        _deviceList = new SynchronizedViewMirror<KeyValuePair<Slug<SnakeSlugFormatter>, RemoteDevice>, DeviceViewModel>(Remote.Devices.CreateView(kvp => new DeviceViewModel(kvp.Value, bindingsRegistry, slotRegistry)));
        DevicesView = _deviceList.Output;

        Remote.RemoteInfo.AsObservable().Subscribe((v) =>
        {
            RemoteInfoControlViewModel? n = v != null ? new RemoteInfoControlViewModel(v) : null;
            
            InfoViewModelStream.OnNext(n);
            OnPropertyChanged(nameof(InfoViewModel));
        }).AddTo(_disposables);
        
        Remote.ConnectionStatus.AsObservable().Skip(1).Subscribe((b) =>
        {
            if (b.State.IsDisconnected())
            {
                TearDown();
            }
        }).AddTo(_disposables);
    }

    [RelayCommand]
    public void SwitchToPage()
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
                Pipeline.PipelineBuilder.Build().RunAsync(new RemoteConnectionPipeline.TaskContexts.ConnectionContext(_serviceProvider,
                                               Remote), _connectionCts.Token);
        }
        catch (OperationCanceledException)
        {
            
        }
        catch (Exception e)
        {
            await Remote.Socket.SendPacketAsync(new DisconnectPacket(new TranslatableTextComponent("UniScan:i18n/disconnect/client_exception_thrown")));
            
            ((IRemoteServerMutationProxy)Remote).SetConnectionStatus(new UnexpectedDisconnectedConnectionStatusContext(e));
            _connectionCts.Cancel();
        }
    }
    
    [RelayCommand]
    public async Task DisconnectAsync()
    {
        if (Remote.ConnectionStatus.Value.State.IsDisconnected())
        {
            return;
        }
        
        ((IRemoteServerMutationProxy)Remote).SetConnectionStatus(new DefaultConnectionStatusContext(ConnectionState.UserDisconnected));
        
        _connectionCts?.Cancel();
        
        await Remote.Socket.SendPacketAsync(new DisconnectPacket(new TranslatableTextComponent("UniScan:i18n/disconnect/user_disconnect")));
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
        
        _deviceList.Dispose();
        DeviceListControl?.Dispose();
        
        InfoViewModelStream.Dispose();
        
        RootPageViewModel.Dispose();
    }
}