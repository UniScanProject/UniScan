using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Client.App.ViewModels.Controls;
using UniScan.Client.App.ViewModels.Pages;
using UniScan.Client.Core;
using UniScan.Client.Core.Config.Types;
using UniScan.Network.Util;

namespace UniScan.Client.App.ViewModels;

public class RemoteViewModel : SubPagedViewModelBase, IDisposable
{
    public RemoteServer Remote { get; set; }

    private readonly NotConnectedRemotePageViewModel _notConnectedPage;
    private MainRemotePageViewModel? _mainPage;
    
    public DeviceListViewModel DeviceList { get; }

    public RemoteViewModel(RemoteServer remote) : base(new NotConnectedRemotePageViewModel(remote), UniScanApp.Identifier.Derived("view_model", "remote", new Slug<SnakeSlugFormatter>(Guid.NewGuid().ToString())))
    {
        this._notConnectedPage = (NotConnectedRemotePageViewModel)CurrentSubpage;
        this.Remote = remote;
        
        DeviceList = new DeviceListViewModel(remote);

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
            
            this.CurrentSubpage = new DisconnectedRemotePageViewModel(reason)
            {
                OkClicked = new RelayCommand(() =>
                {
                    this.CurrentSubpage = _notConnectedPage;
                })
            };

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
            _mainPage ??= new MainRemotePageViewModel(Remote, DeviceList);
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
        
        DeviceList.Dispose();
    }
}