using System;
using System.Collections.Specialized;
using ObservableCollections;

namespace UniScan.Client.App.Core.Helpers;

//literally a hack to create a new observable list from a view so that I can derive more views
public class SynchronizedViewMirror<T, TNew> : IDisposable
{
    private readonly ObservableList<TNew> _output = [];
    public IReadOnlyObservableList<TNew> Output => _output;

    private readonly ISynchronizedView<T, TNew> _view;
    
    public SynchronizedViewMirror(ISynchronizedView<T, TNew> view)
    {
        _view = view;
        
        Reset();
        Attach();
    }
    
    private void Attach()
    {
        _view.ViewChanged += OnViewChanged;
    }

    private void Detach()
    {
        _view.ViewChanged -= OnViewChanged;
    }

    private void Reset()
    {
        _output.Clear();
        _output.AddRange(_view);
    }

    private void OnViewChanged(in SynchronizedViewChangedEventArgs<T, TNew> args)
    {
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (args.IsSingleItem)
                {
                    _output.Add(args.NewItem.View);
                    break;
                }
                
                _output.AddRange(args.NewViews);
                break;
            case NotifyCollectionChangedAction.Remove:
                if (args.IsSingleItem)
                {
                    _output.Remove(args.OldItem.View);
                    break;
                }
                
                foreach (TNew v in args.OldViews)
                {
                    _output.Remove(v);
                }

                break;
            case NotifyCollectionChangedAction.Replace:
                _output[args.NewStartingIndex] = args.NewItem.View;
                break;
            case NotifyCollectionChangedAction.Move:
                _output.Move(args.OldStartingIndex, args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Reset:
                _output.Clear();
                break;
            default:
                break;
        }
    }

    public void Dispose()
    {
        this.Detach();
        _output.Clear();
    }
}