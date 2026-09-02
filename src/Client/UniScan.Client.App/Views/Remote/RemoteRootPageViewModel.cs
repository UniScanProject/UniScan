using System;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using R3;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.Global;
using UniScan.Client.App.Views.Remote.Connection;
using UniScan.Client.App.Views.Remote.Device;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Device;
using UniScan.Network.Util;

namespace UniScan.Client.App.Views.Remote;

public class RemoteRootPageViewModel : SubPagedViewModelBase, IDisposable, IRemoteRootPageDeviceNavigatorProxy
{
    public RemoteServer Remote { get; set; }

    private readonly NotConnectedRemotePageViewModel _notConnectedPage;
    private RemotePageViewModel? _mainPage;
    
    public DeviceListControlViewModel DeviceListControl { get; }
    
    public BehaviorSubject<RemoteInfoControlViewModel?> InfoViewModelStream { get; } = new(null); 
    public RemoteInfoControlViewModel? InfoViewModel => InfoViewModelStream.Value;

    public INotifyCollectionChangedSynchronizedViewList<DeviceRootPageViewModel> DevicePages { get; }

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
        DeviceListControl = new DeviceListControlViewModel(remote, this);

        this.Remote.Socket.ConnectionState.Disconnected += OnDisconnected;
        this.Remote.Socket.ConnectionState.Connected += OnConnected;
    }

    private void OnDisconnected(object? sender, ConnectionStateTracker.ConnectionStateChangedEventArgs eventArgs)
    {
        Dispatcher.UIThread.Post(() =>
        {
            string reason = "Unknown reason, maybe there is more info in the logs.";
            if (eventArgs.Channel.HasAttribute(ServerAttributes.DisconnectReasonAttribute))
            {
                reason = eventArgs.Channel.GetAttribute(ServerAttributes.DisconnectReasonAttribute).Get();
            }
            
            this.CurrentSubpage = new DisconnectedRemotePageViewModel(reason, Remote)
            {
                OkClicked = new RelayCommand(() =>
                {
                    this.CurrentSubpage = _notConnectedPage;
                })
            };

            //todo can we PLEASE manage state better!!!!!!
            if (_mainPage is IDisposable d)
            {
                d.Dispose();
            }
            _mainPage = null;
        });
        
    }
    
    private void OnConnected(object? sender, ConnectionStateTracker.ConnectionStateChangedEventArgs eventArgs)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _mainPage ??= new RemotePageViewModel(Remote, DeviceListControl, InfoViewModelStream.AsObservable());
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
        
        DeviceListControl.Dispose();
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