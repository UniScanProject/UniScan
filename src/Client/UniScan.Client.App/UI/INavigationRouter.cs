using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Client.App.UI;

public interface INavigationRouter<in TViewModel>
    where TViewModel : ObservableObject
{
    bool CanGoBack { get; }
    IRelayCommand GoBackCommand { get; }
    
    void Navigate(TViewModel vm, bool clear = false);
}

public partial class NavigationRouter<TViewModel>(ISubPagedViewNavigator host)
    : ObservableObject, INavigationRouter<TViewModel>
    where TViewModel : ObservableObject
{
    private readonly Stack<TViewModel> _stack = new();
    
    [ObservableProperty]
    public partial bool CanGoBack { get; private set; }

    public void Navigate(TViewModel vm, bool clear = false)
    {
        if (clear)
        {
            _stack.Clear();
        }
        else if (host is ISubPagedViewModel { CurrentSubpage: TViewModel currentSubpage })
        {
            _stack.Push(currentSubpage);
        }
        
        CanGoBack = _stack.Count > 0;
        host.Navigate(vm);
    }

    [RelayCommand]
    private void GoBack()
    {
        if (_stack.TryPop(out TViewModel? previous))
        {
            CanGoBack = _stack.Count > 0;
            host.Navigate(previous);
        }
    }
}