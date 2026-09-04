using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using ObservableCollections;
using Serilog;
using Shiki.Common.Identity;
using UniScan.Client.App.Core.Helpers;
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

public partial class MainViewModel : SingletonSubPagedViewModelBase<MainViewModel>, ISingletonSubPagedViewModel, IDisposable
{
    public ClientSettingsViewModel Settings { get; }
    
    public IRemoteManager RemoteManager { get; }
    public IRemoteFactory RemoteFactory { get; }
    
    public new static Identifier Identifier { get; } = UniScanApp.Identifier.Derived("view_model", "main");
    
    private readonly ISynchronizedView<RemoteServer, RemoteViewModel> _remotesView;
    public SynchronizedViewMirror<RemoteServer, RemoteViewModel> RemoteViewList { get; }
    
    private readonly HomePageViewModel _mainPage;
    
    public MainViewModel(IServiceProvider provider, IRemoteManager remoteManager, ClientSettingsViewModel clientSettingsViewModel, IRemoteFactory remoteFactory) : base(new EmptyPageViewModel())
    {
        RemoteManager = remoteManager;
        RemoteFactory = remoteFactory;
        Settings = clientSettingsViewModel;

        _remotesView = RemoteManager.Remotes.CreateView(remote => new RemoteViewModel(remote, provider));
        RemoteViewList = new SynchronizedViewMirror<RemoteServer, RemoteViewModel>(_remotesView);
        
        _remotesView.ViewChanged += (in args) =>
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (RemoteViewModel vm in args.OldViews)
                    {
                        if (CurrentSubpage is RemoteRootPageViewModel rvm &&
                            rvm.RemoteViewModel == vm)
                        {
                            OnHomeClicked();
                        }

                        vm.Dispose();
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    OnHomeClicked();
                    break;
            }
        };
        
        _mainPage = new HomePageViewModel(RemoteViewList.Output);
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
            Log.Information("Added new remote {Remote}", vm.CreatedRemote);
        }
    }

    [RelayCommand]
    public async Task OnRemoveRemoteClicked(RemoteViewModel vm)
    {
        RemoteManager.Remotes.Remove(vm.Remote);
        Log.Information("Removed remote '{Remote}'", vm.Remote.DisplayName);
    }
    
    [RelayCommand]
    public void OnSettingsClicked() => CurrentSubpage = Settings;

    public void Dispose()
    {
        _remotesView.Dispose();
        _mainPage.Dispose();
        
        foreach (RemoteViewModel rvm in RemoteViewList.Output)
        {
            rvm.Dispose();
        }
        RemoteViewList.Dispose();
    }
}