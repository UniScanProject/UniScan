using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using ObservableCollections;
using UniScan.Client.App.UI.ConnectionMethod;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Client.Core.DI.Factory;
using UniScan.Client.Core.Remote;
using UniScan.Network.Client.Remote.Connection;

namespace UniScan.Client.App.Views.Remote.Action;

public partial class AddRemoteDialogViewModel : ViewModelBase
{
    private readonly IRemoteFactory _remoteFactory;
    
    [ObservableProperty]
    public partial ConnectionMethodFactoryViewModelSource.ConnectionMethodViewModelFactoryPair? SelectedMethodFactory { get; set; }
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    [NotifyPropertyChangedFor(nameof(HasMethodFactory))]
    public partial IConnectionMethodFactoryViewModel? CurrentMethodFactory { get; set; }
    
    public bool HasMethodFactory => CurrentMethodFactory != null;

    public ObservableList<ConnectionMethodFactoryViewModelSource.ConnectionMethodViewModelFactoryPair> ViewModels { get; }

    public RemoteServer CreatedRemote { get; private set; } = null!;
    
    public AddRemoteDialogViewModel(IRemoteFactory factory)
    {
        _remoteFactory = factory;
        ViewModels = [.. ConnectionMethodFactoryViewModelSource.Get()];
    }
    
    [RelayCommand(CanExecute = nameof(CanSubmit))]
    public void Confirm()
    {
        IRemoteConnectionMethod? method = CurrentMethodFactory?.Create();
        if (method == null)
        {
            return;
        }
        
        CreatedRemote = _remoteFactory.Create(Guid.NewGuid(), method);
        DialogHost.Close("MainDialogHost", CreatedRemote);
    }
    
    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close("MainDialogHost");
    }

    private bool CanSubmit()
    {
        return CurrentMethodFactory is { IsValid: true };
    }
    
    partial void OnSelectedMethodFactoryChanged(ConnectionMethodFactoryViewModelSource.ConnectionMethodViewModelFactoryPair? value)
    {
        if (value == null)
        {
            CurrentMethodFactory = null;
            return;
        }

        CurrentMethodFactory = (IConnectionMethodFactoryViewModel?)Activator.CreateInstance(value.Type);
    }
    
    partial void OnCurrentMethodFactoryChanged(IConnectionMethodFactoryViewModel? oldValue,
                                               IConnectionMethodFactoryViewModel? newValue)
    {
        if (oldValue is INotifyPropertyChanged old)
        {
            old.PropertyChanged -= CurrentMethodFactoryChanged;
        }
        
        if (newValue is INotifyPropertyChanged n)
        {
            n.PropertyChanged += CurrentMethodFactoryChanged;
        }
    }

    private void CurrentMethodFactoryChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IConnectionMethodFactoryViewModel.IsValid))
        {
            ConfirmCommand.NotifyCanExecuteChanged();
        }
    }
}