using System;
using System.Collections.Specialized;
using ObservableCollections;

namespace UniScan.Client.App.Core.Helpers;

//literally a hack to create a new observable list from a view so that I can derive more views
public class SynchronizedViewMirror<T, TNew> : IDisposable
{
    private readonly ObservableList<TNew> _output = [];
    
    
    public IReadOnlyObservableList<TNew> Output => _output;
    public INotifyCollectionChangedSynchronizedViewList<TNew> OutputView { get; }

    private readonly ISynchronizedView<T, TNew> _view;
    
    public SynchronizedViewMirror(ISynchronizedView<T, TNew> view)
    {
        _view = view;

        OutputView = Output.CreateView(x => x)
                            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        
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
                    //we lost the plot man
                    if (args.NewStartingIndex != -1)
                    {
                        _output.Insert(args.NewStartingIndex, args.NewItem.View);
                    }
                    else
                    {
                        _output.Add(args.NewItem.View);
                    }

                    break;
                }

                if (args.NewStartingIndex != -1)
                {
                    _output.InsertRange(args.NewStartingIndex, args.NewViews);
                }
                else
                {
                    _output.AddRange(args.NewViews);
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                if (args.IsSingleItem)
                {
                    if (args.OldStartingIndex != -1)
                    {
                        _output.RemoveAt(args.OldStartingIndex);
                    }
                    else
                    {
                        _output.Remove(args.OldItem.View);
                    }

                    break;
                }

                if (args.OldStartingIndex != -1)
                {
                    _output.RemoveRange(args.OldStartingIndex, args.OldViews.Length);
                }
                else
                {
                    foreach (TNew v in args.OldViews)
                    {
                        _output.Remove(v);
                    }
                }

                break;
            case NotifyCollectionChangedAction.Replace:
                if (args.IsSingleItem)
                {
                    _output[args.NewStartingIndex] = args.NewItem.View;
                    break;
                }
                
                //TODO

                break;
            case NotifyCollectionChangedAction.Move:
                if (args.NewStartingIndex != -1 || args.OldStartingIndex != -1)
                {
                    Reset();//just rebuild the whole thing because you're fucked if this happens somehow
                    return;
                }
                
                _output.Move(args.OldStartingIndex, args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Reset:
                Reset();
                break;
            default:
                break;
        }
    }

    public void Dispose()
    {
        Detach();
        _output.Clear();
        OutputView.Dispose();
    }
}