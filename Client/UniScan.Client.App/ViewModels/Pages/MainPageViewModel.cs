using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;
using UniScan.Client.Core.Config.Remote;
using UniScan.Client.Core.Config.Types;

namespace UniScan.Client.App.ViewModels.Pages;

public class RemoteConnectionStateFilter : ISynchronizedViewFilter<RemoteServer, RemoteViewModel>
{
    public bool IsMatch(RemoteServer value, RemoteViewModel view)
    {
        return value.Connected.CurrentValue;
    }
}

public partial class MainPageViewModel : ViewModelBase, IDisposable
{
    public INotifyCollectionChangedSynchronizedViewList<RemoteViewModel> ConnectedRemotesView { get; }
    private readonly ISynchronizedView<RemoteServer, RemoteViewModel> _remotesView;

    private readonly Dictionary<RemoteServer, IDisposable> _subscriptions = [];

    private readonly IRemoteManager _remoteManager;
    
    public MainPageViewModel(IRemoteManager remoteManager)
    {
        _remoteManager = remoteManager;
        
        _remotesView = remoteManager.Remotes.CreateView(remote => new RemoteViewModel(remote));
        _remotesView.AttachFilter(new RemoteConnectionStateFilter());

        ConnectedRemotesView = _remotesView.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

        foreach (RemoteServer remoteViewModel in remoteManager.Remotes)
        {
            Track(remoteViewModel);
        }
        _remoteManager.Remotes.CollectionChanged += OnRemotesChanged;
    }

    private void Track(RemoteServer remote)
    {
        if (_subscriptions.ContainsKey(remote)) return;

        _subscriptions[remote] = remote.Connected.Skip(1).Subscribe((_) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _remotesView.AttachFilter(new RemoteConnectionStateFilter());
            });
        });
    }

    private void Untrack(RemoteServer remote)
    {
        if (_subscriptions.TryGetValue(remote, out IDisposable? subscription))
        {
            subscription.Dispose();
            _subscriptions.Remove(remote);
        }
    }

    private void OnRemotesChanged(in NotifyCollectionChangedEventArgs<RemoteServer> e)
    {
        if (!e.NewItems.IsEmpty)
        {
            foreach (RemoteServer newItem in e.NewItems)
            {
                Track(newItem);
            }
        }

        if (!e.OldItems.IsEmpty)
        {
            foreach (RemoteServer remote in e.OldItems)
            {
                Untrack(remote);
            }
        }
    }


    public void Dispose()
    {
        _remoteManager.Remotes.CollectionChanged -= OnRemotesChanged;
        
        foreach (IDisposable subscription in _subscriptions.Values)
        {
            subscription.Dispose();
        }
        _subscriptions.Clear();
        
        ConnectedRemotesView.Dispose();
        _remotesView.Dispose();
    }
}