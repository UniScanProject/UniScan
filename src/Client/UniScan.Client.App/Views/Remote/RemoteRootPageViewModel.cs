using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using R3;
using Serilog;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.Global;
using UniScan.Client.App.Views.Remote.Connection;
using UniScan.Client.App.Views.Remote.Device;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Device;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Util;

namespace UniScan.Client.App.Views.Remote;

public class RemoteRootPageViewModel : SubPagedViewModelBase, IDisposable, IRemoteRootPageDeviceNavigatorProxy
{
    public RemoteServer Remote { get; }

    private readonly NotConnectedRemotePageViewModel _notConnectedPage;
    private RemotePageViewModel? _mainPage;
    public DeviceListControlViewModel? DeviceListControl { get; private set; }
    
    public BehaviorSubject<RemoteInfoControlViewModel?> InfoViewModelStream { get; } = new(null); 
    public RemoteInfoControlViewModel? InfoViewModel => InfoViewModelStream.Value;

    public INotifyCollectionChangedSynchronizedViewList<DeviceRootPageViewModel> DevicePages { get; }

    private bool _userDisconnected = false;

    public RemoteRootPageViewModel(IServiceProvider provider, RemoteServer remote) : base(new NotConnectedRemotePageViewModel(provider, remote), UniScanApp.Identifier.Derived("view_model", "remote", new Slug<SnakeSlugFormatter>(Guid.NewGuid().ToString())))
    {
        this._notConnectedPage = (NotConnectedRemotePageViewModel)CurrentSubpage;
        this._notConnectedPage.OnConnecting += (pipeline) =>
        {
            this.CurrentSubpage = new LoadingViewModel("Connecting...", pipeline.Pipeline);
        };

        this._notConnectedPage.OnConnectFailed += (ex) =>
        {
            this.CurrentSubpage = new DisconnectedRemotePageViewModel("Failed to connect to server! " + ex.Message, Remote)
            {
                OkClicked = new RelayCommand(() =>
                {
                    this.CurrentSubpage = _notConnectedPage;
                })
            };
        };
        
        this.Remote = remote;
        Remote.RemoteInfo.Subscribe((v) =>
        {
            RemoteInfoControlViewModel? n = v != null ? new RemoteInfoControlViewModel(v) : null;
            
            InfoViewModelStream.OnNext(n);
            OnPropertyChanged(nameof(InfoViewModel));
        });

        DevicePages = Remote.Devices.CreateView(kvp => new DeviceRootPageViewModel(provider, kvp.Value)).ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

        this.Remote.Socket.ConnectionState.Disconnected += OnDisconnected;
        this.Remote.Socket.ConnectionState.Connected += OnConnected;
    }

    private void OnDisconnected(object? sender, ConnectionStateTracker.ConnectionStateChangedEventArgs eventArgs)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!_userDisconnected)
            {
                string reason = "Unknown reason, maybe there is more info in the logs.";
                if (eventArgs.Channel.HasAttribute(ServerAttributes.DisconnectReasonAttribute))
                {
                    reason = eventArgs.Channel.GetAttribute(ServerAttributes.DisconnectReasonAttribute).Get();
                }

                this.CurrentSubpage = new DisconnectedRemotePageViewModel(reason, Remote)
                {
                    OkClicked = new RelayCommand(() => { this.CurrentSubpage = _notConnectedPage; })
                };
            }

            _userDisconnected = false;

            TearDown();
        });
    }

    public async Task Disconnect()
    {
        if (!Remote.Connected.Value)
        {
            return;
        }
        
        Dispatcher.UIThread.Post(() =>
        {
            this.CurrentSubpage = _notConnectedPage;
            _userDisconnected = true;
        });
        
        await Remote.Socket.SendPacketAsync(new DisconnectPacket("User initiated disconnect"));
        await Task.Delay(500);//wait for server to disconnect
        await Remote.Socket.StopAsync();
    } 

    private void TearDown()
    {
        DeviceListControl?.Dispose();
        DeviceListControl = null;
            
        _mainPage?.Dispose();
        _mainPage = null;
    }
    
    private void OnConnected(object? sender, ConnectionStateTracker.ConnectionStateChangedEventArgs eventArgs)
    {
        Dispatcher.UIThread.Post(() =>
        {
            DeviceListControl = new DeviceListControlViewModel(Remote, this);
            
            _mainPage = new RemotePageViewModel(Remote, DeviceListControl, InfoViewModelStream.AsObservable());
            this.CurrentSubpage = _mainPage;
        });
    }

    public void Dispose()
    {
        if (this.Remote?.Socket?.ConnectionState is { } state)
        {
            state.Disconnected -= OnDisconnected;
            state.Connected -= OnConnected;
        }
        
        TearDown();
        DevicePages.Dispose();
    }

    void IRemoteRootPageDeviceNavigatorProxy.Navigate(RemoteDevice device)
    {
        DeviceRootPageViewModel? p = DevicePages.FirstOrDefault(page => page.Device.Identifier == device.Identifier);
        if (p != null)
        {
            Dispatcher.UIThread.Post(() => { CurrentSubpage = p; });
        }
    }
}

public interface IRemoteRootPageDeviceNavigatorProxy
{
    void Navigate(RemoteDevice device);
}