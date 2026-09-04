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
    public RemoteViewModel RemoteViewModel { get; }

    private readonly NotConnectedRemotePageViewModel _notConnectedPage;
    private RemotePageViewModel? _mainPage;
    
    private readonly CompositeDisposable _disposables = new();
    
    public RemoteRootPageViewModel(RemoteViewModel remoteViewModel) : base(new NotConnectedRemotePageViewModel(remoteViewModel), UniScanApp.Identifier.Derived("view_model", "remote", new Slug<SnakeSlugFormatter>(remoteViewModel.Remote.Id.ToString())))
    {
        this._notConnectedPage = (NotConnectedRemotePageViewModel)CurrentSubpage;
        
        this.RemoteViewModel = remoteViewModel;

        this.RemoteViewModel.Remote.ConnectionStatus.AsObservable().Skip(1).Subscribe(OnConnectionStateChanged).AddTo(_disposables);
    }

    private void OnDisconnected(IConnectionStatusContext ctx)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (ctx.State == ConnectionState.UserDisconnected)
            {
                this.CurrentSubpage = _notConnectedPage;
                
                TearDown();
                return;
            }

            string reason = ctx switch
            {
                KickedDisconnectedConnectionStatusContext c     => c.Reason,
                UnexpectedDisconnectedConnectionStatusContext c => c.Exception?.Message,
                _                                                      => null
            } ?? "Unknown disconnect reason";

            this.CurrentSubpage = new DisconnectedRemotePageViewModel(reason, RemoteViewModel)
            {
                OkClicked = new RelayCommand(() => { this.CurrentSubpage = _notConnectedPage; })
            };
            

            TearDown();
        });
    }
    
    private void OnConnected(IConnectionStatusContext ctx)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _mainPage = new RemotePageViewModel(RemoteViewModel);
            this.CurrentSubpage = _mainPage;
        });
    }

    private void TearDown()
    {
        _mainPage?.Dispose();
        _mainPage = null;
    }
    
    public void Dispose()
    {
        _disposables.Dispose();
        TearDown();
    }

    void IRemoteRootPageDeviceNavigatorProxy.Navigate(RemoteDevice device)
    {
        DeviceRootPageViewModel? p = RemoteViewModel.DevicePages.FirstOrDefault(page => page.Device.Identifier == device.Identifier);
        if (p != null)
        {
            Dispatcher.UIThread.Post(() => { CurrentSubpage = p; });
        }
    }

    private void OnConnectionStateChanged(IConnectionStatusContext ctx)
    {
        switch (ctx.State)
        {
            case ConnectionState.Connecting:
                CurrentSubpage = new LoadingViewModel("Connecting...", RemoteViewModel.Pipeline.Pipeline);
                break;
            case ConnectionState.Connected:
                OnConnected(ctx);
                break;
            case ConnectionState.Disconnected:
            case ConnectionState.UserDisconnected:
            case ConnectionState.UnexpectedDisconnected:
            case ConnectionState.KickedDisconnected:
                OnDisconnected(ctx);
                break;
            case ConnectionState.Handshaking:
            default:
                break;
        }
    }
}

public interface IRemoteRootPageDeviceNavigatorProxy
{
    void Navigate(RemoteDevice device);
}