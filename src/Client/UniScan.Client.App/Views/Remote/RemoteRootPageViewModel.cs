using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using R3;
using Serilog;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Internationalization.Component;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.Global;
using UniScan.Client.App.Views.Remote.Connection;
using UniScan.Client.App.Views.Remote.Device;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Connection;
using UniScan.Client.Core.Remote.Connection.Status;
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
    
    public IRelayCommand NavigateCommand { get; }

    public RemoteRootPageViewModel(RemoteViewModel remoteViewModel) : base(new NotConnectedRemotePageViewModel(remoteViewModel), UniScanApp.Identifier.Derived("view_model", "remote", new Slug<SnakeSlugFormatter>(remoteViewModel.Remote.Id.ToString())))
    {
        _notConnectedPage = (NotConnectedRemotePageViewModel)CurrentSubpage;
        
        RemoteViewModel = remoteViewModel;

        RemoteViewModel.Remote.ConnectionStatus.AsObservable().Skip(1).Subscribe(OnConnectionStateChanged).AddTo(_disposables);
        NavigateCommand = new RelayCommand<RemoteDevice?>(((IRemoteRootPageDeviceNavigatorProxy)this).Navigate);
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

    void IRemoteRootPageDeviceNavigatorProxy.Navigate(RemoteDevice? device)
    {
        if (device == null)//TODO THIS IS SHIT BECAUSE THIS WILL SWITCH THE USER OUT OF WHATEVER PAGE REGARDLESS WHEN THE DEVICE IS UNSUBSCRIBED BY THE SERVER!!!!! WE DONT WANT SUCH SPECIFIC SHIT LIKE THIS BUT RUSHING THIS
        {
            Dispatcher.UIThread.Post(() => { CurrentSubpage = (ObservableObject?)_mainPage ?? _notConnectedPage; });
            return;
        }

        DeviceViewModel? p = RemoteViewModel.DevicesView.FirstOrDefault(page => page.Device.Identifier == device.Identifier);
        if (p != null)
        {
            Dispatcher.UIThread.Post(() => { CurrentSubpage = p.RootPageViewModel; });
        }
    }

    private void OnDisconnected(IConnectionStatusContext ctx)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (ctx.State == ConnectionState.UserDisconnected)
            {
                CurrentSubpage = _notConnectedPage;
                
                TearDown();
                return;
            }

            ITextComponent? reason = ctx switch
            {
                KickedDisconnectedConnectionStatusContext c     => c.Reason,
                UnexpectedDisconnectedConnectionStatusContext c => new TranslatableTextComponent("UniScan:client/i18n/disconnect_reason/exception_thrown", c.Exception),
                _                                               => null
            } ?? new TranslatableTextComponent("UniScan:client/i18n/disconnect_reason/no_reason");

            CurrentSubpage = new DisconnectedRemotePageViewModel(reason, RemoteViewModel)
            {
                OkClicked = new RelayCommand(() => { CurrentSubpage = _notConnectedPage; })
            };
            
            TearDown();
        });
    }

    private void OnConnecting(IConnectionStatusContext ctx)
    {
        Dispatcher.UIThread.Post(() =>
        {
            CurrentSubpage = new LoadingViewModel("Connecting...", RemoteViewModel.Pipeline.PipelineBuilder.ExecutionState);
        });
    }
    
    private void OnConnected(IConnectionStatusContext ctx)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _mainPage = new RemotePageViewModel(RemoteViewModel);
            CurrentSubpage = _mainPage;
        });
    }
    
    private void OnConnectionStateChanged(IConnectionStatusContext ctx)
    {
        switch (ctx.State)
        {
            case ConnectionState.Connecting:
                OnConnecting(ctx);
                break;
            case ConnectionState.Connected:
                OnConnected(ctx);
                break;
            case ConnectionState.NotConnected:
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
    IRelayCommand NavigateCommand { get; }
    
    void Navigate(RemoteDevice? device);
}