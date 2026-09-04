using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ObservableCollections;
using R3;
using UniScan.Client.App.Views.Remote;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Connection;

namespace UniScan.Client.App.Views.Home;

public class RemoteConnectionStateFilter : ISynchronizedViewFilter<RemoteViewModel, RemoteViewModel>
{
    public bool IsMatch(RemoteViewModel value, RemoteViewModel rootPageView)
    {
        return value.Remote.ConnectionStatus.Value.State is ConnectionState.Connected;
    }
}

public partial class HomePageViewModel : ViewModelBase, IDisposable
{
    public INotifyCollectionChangedSynchronizedViewList<RemoteViewModel> ConnectedRemotesView { get; }
    private readonly ISynchronizedView<RemoteViewModel, RemoteViewModel> _connectedRemotesView;

    private readonly IReadOnlyObservableList<RemoteViewModel> _remotes;

    private readonly Dictionary<RemoteViewModel, IDisposable> _subscriptions = [];
    
    public HomePageViewModel(IReadOnlyObservableList<RemoteViewModel> viewModels)
    {
        _remotes = viewModels;
        
        _connectedRemotesView = viewModels.CreateView(v => v);
        _connectedRemotesView.AttachFilter(new RemoteConnectionStateFilter());
        
        ConnectedRemotesView = _connectedRemotesView.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        
        foreach (RemoteViewModel vm in viewModels)
        {
            Track(vm);
        }
        viewModels.CollectionChanged += OnRemotesChanged;
    }

    private void Track(RemoteViewModel remote)
    {
        if (_subscriptions.ContainsKey(remote)) return;

        _subscriptions[remote] = remote.Remote.ConnectionStatus.AsObservable().Skip(1).Subscribe(_ =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _connectedRemotesView.AttachFilter(new RemoteConnectionStateFilter());
            });
        });
    }

    private void Untrack(RemoteViewModel remote)
    {
        if (_subscriptions.TryGetValue(remote, out IDisposable? subscription))
        {
            subscription.Dispose();
            _subscriptions.Remove(remote);
        }
    }

    private void OnRemotesChanged(in NotifyCollectionChangedEventArgs<RemoteViewModel> e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.IsSingleItem)
                {
                    Track(e.NewItem);
                    break;
                }
                
                foreach (RemoteViewModel vm in e.NewItems)
                {
                    Track(vm);
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.IsSingleItem)
                {
                    Untrack(e.OldItem);
                    break;
                }
                
                foreach (RemoteViewModel vm in e.OldItems)
                {
                    Untrack(vm);
                }

                break;
            case NotifyCollectionChangedAction.Reset:
                foreach (IDisposable s in _subscriptions.Values)
                {
                    s.Dispose();
                }
                _subscriptions.Clear();
                
                foreach (RemoteViewModel vm in _connectedRemotesView)
                {
                    Track(vm);
                }

                break;
        }
    }

    public void Dispose()
    {
        _remotes.CollectionChanged -= OnRemotesChanged;
        
        foreach (IDisposable subscription in _subscriptions.Values)
        {
            subscription.Dispose();
        }
        _subscriptions.Clear();
        
        ConnectedRemotesView.Dispose();
        _connectedRemotesView.Dispose();
    }
}