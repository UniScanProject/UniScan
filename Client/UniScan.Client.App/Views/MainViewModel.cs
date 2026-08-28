using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ObservableCollections;
using Serilog;
using Shiki.Common.Identity;
using UniScan.Client.App.Views.Global;
using UniScan.Client.App.Views.Home;
using UniScan.Client.App.Views.Remote;
using UniScan.Client.App.Views.Remote.Action;
using UniScan.Client.App.Views.Settings;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core;
using UniScan.Client.Core.DI.Factory;
using UniScan.Client.Core.Remote;

namespace UniScan.Client.App.Views;

public partial class MainViewModel : SingletonSubPagedViewModelBase<MainViewModel>, ISingletonSubPagedViewModel
{
    public ClientSettingsViewModel Settings { get; }
    public UniScanClient Client { get; }
    
    public IRemoteManager RemoteManager { get; }
    public IRemoteFactory RemoteFactory { get; }
    
    public new static Identifier Identifier { get; } = UniScanApp.Identifier.Derived("view_model", "main");
    
    public INotifyCollectionChangedSynchronizedViewList<SidebarRemoteItemControlViewModel> RemotesView { get; }

    private readonly HomePageViewModel _mainPage;
    
    public MainViewModel(IServiceProvider provider, IRemoteManager remoteManager, ClientSettingsViewModel clientSettingsViewModel, IRemoteFactory remoteFactory) : base(new EmptyPageViewModel())
    {
        RemoteManager = remoteManager;
        RemoteFactory = remoteFactory;
        Settings = clientSettingsViewModel;
    
        RemotesView = RemoteManager.Remotes.CreateView(remote => new SidebarRemoteItemControlViewModel(provider, remote))
                                   .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);

        RemotesView.CollectionChanged += (sender, args) =>
        {
            if (args is not { Action: NotifyCollectionChangedAction.Remove, OldItems: not null })
                return;
            
            foreach (SidebarRemoteItemControlViewModel vm in args.OldItems)
            {
                if (CurrentSubpage is RemoteRootPageViewModel rvm && rvm.Remote == vm.Remote)
                {
                    OnHomeClicked();
                }

                vm.Dispose();
            }
        };
        
        _mainPage = new HomePageViewModel(provider, RemoteManager);
        CurrentSubpage = _mainPage;
    }
    
    [RelayCommand]
    public void OnHomeClicked() => CurrentSubpage = _mainPage;

    [RelayCommand]
    public async Task OnAddRemoteClicked()
    {
        AddRemoteDialogViewModel vm = new(RemoteFactory);
        AddRemoteDialog dialog = new()
        {
            DataContext = vm
        };

        object? result = await DialogHostAvalonia.DialogHost.Show(dialog, "MainDialogHost");
        
        if (vm.CreatedRemote != null)
        {
            RemoteManager.Remotes.Add(vm.CreatedRemote);
            Log.Logger.Information("Added new remote {Remote}", vm.CreatedRemote);
        }
    }

    [RelayCommand]
    public async Task OnRemoveRemoteClicked(SidebarRemoteItemControlViewModel? rcvm)
    {
        if (rcvm?.Remote == null)
            throw new NullReferenceException("Remote is null, how?");
        
        RemoteManager.Remotes.Remove(rcvm.Remote);
        Log.Logger.Information("Removed remote {Remote}", rcvm.Remote);
    }
    
    [RelayCommand]
    public void OnSettingsClicked() => CurrentSubpage = Settings;
}