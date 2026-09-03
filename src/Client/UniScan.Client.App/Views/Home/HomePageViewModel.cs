using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;
using UniScan.Client.App.Views.Remote;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;

namespace UniScan.Client.App.Views.Home;

public class RemoteConnectionStateFilter : ISynchronizedViewFilter<RemoteServer, RemoteRootPageViewModel>
{
    public bool IsMatch(RemoteServer value, RemoteRootPageViewModel rootPageView)
    {
        return value.Connected.Value;
    }
}

public partial class HomePageViewModel : ViewModelBase, IDisposable
{
    public INotifyCollectionChangedSynchronizedViewList<RemoteRootPageViewModel> ConnectedRemotesView { get; }
    private readonly ISynchronizedView<RemoteServer, RemoteRootPageViewModel> _remotesView;

    private readonly Dictionary<RemoteServer, IDisposable> _subscriptions = [];

    private readonly IRemoteManager _remoteManager;
    
    public HomePageViewModel(IServiceProvider provider, IRemoteManager remoteManager)
    {
        _remoteManager = remoteManager;
        
        _remotesView = remoteManager.Remotes.CreateView(remote => new RemoteRootPageViewModel(provider, remote));
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

        _subscriptions[remote] = remote.Connected.AsObservable().Skip(1).Subscribe((_) =>
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