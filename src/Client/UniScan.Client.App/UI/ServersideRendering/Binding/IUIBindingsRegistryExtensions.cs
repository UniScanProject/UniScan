using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using ObservableCollections;
using R3;
using Shiki.Common.Identity;

namespace UniScan.Client.App.UI.ServersideRendering.Binding;

public static class IUIBindingsRegistryExtensions
{
    extension(IUIBindingsRegistry registry)
    {
        // I hate this
        public IDisposable RegisterDict<TProperty>(ObservableDictionary<Identifier, TProperty> properties)
        {
            CompositeDisposable disposable = new(); //also disposes collectionchanged
            CompositeDisposable disposableChildren = new();//for children we are subscribed to
            
            Dictionary<Identifier, BehaviorSubject<TProperty?>> behaviors = new();//children

            disposable.Add(disposableChildren);
            
            foreach (var kvp in properties) RegisterDictValue(kvp.Key, kvp.Value);

            properties.CollectionChanged += OnPropertiesChanged;
            disposable.Add(Disposable.Create(() => properties.CollectionChanged -= OnPropertiesChanged));
            
            return disposable;

            //guh
            void RegisterDictValue(Identifier id, TProperty? value)
            {
                if (!behaviors.TryGetValue(id, out var b))
                {
                    b = new BehaviorSubject<TProperty?>(value);
                    behaviors[id] = b;
                    
                    registry.Register(id, b).AddTo(disposable);
                }
                b.OnNext(value);
            }

            void OnPropertiesChanged(in NotifyCollectionChangedEventArgs<KeyValuePair<Identifier, TProperty>> args)
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Replace:
                        if (args.IsSingleItem)
                        {
                            RegisterDictValue(args.NewItem.Key, args.NewItem.Value);
                            break;
                        }

                        //riddle me this, why can't we just store it in FUCKING NEWITEMS INSTEAD OF HAVING TO CHECK IF ITS A SINGLE ITEM AND HAVE A SPECIAL CASE FOR IT
                        foreach (var kvp in args.NewItems) RegisterDictValue(kvp.Key, kvp.Value);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (args.IsSingleItem)
                        {
                            RegisterDictValue(args.OldItem.Key, default);
                            break;
                        }
                        
                        foreach (var kvp in args.OldItems) RegisterDictValue(kvp.Key, default);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        foreach (IDisposable d in disposableChildren)
                        {
                            d.Dispose();
                        }
                        disposableChildren = new CompositeDisposable();
                        disposable.Add(disposableChildren);
                        
                        behaviors.Clear();
                    
                        foreach (var kvp in properties) RegisterDictValue(kvp.Key, kvp.Value);
                        break;
                    case NotifyCollectionChangedAction.Move://too lazy I don't think this will ever happen... if it does, Too Bad!
                    default:
                        break;
                }
            }
        }
    }
}