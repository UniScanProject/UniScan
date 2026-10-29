using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UniScan.Client.App.Views.ViewModel;
using Shiki.Settings;
using Shiki.Settings.Category;
using UniScan.Client.App.UI;

namespace UniScan.Client.App.Views.Settings.Pages;

public partial class SettingsCategoryPageViewModel : ViewModelBase
{
    public SettingsCategoryNode Node { get; }
    private readonly INavigationRouter<ObservableObject> _router;

    public SettingsCategoryPageViewModel(SettingsCategoryNode node, INavigationRouter<ObservableObject> router)
    {
        Node = node;
        _router = router;
    }

    [RelayCommand]
    private void NavigateToChild(SettingsCategoryNode child)
    {
        _router.Navigate(new SettingsCategoryPageViewModel(child, _router));
    }
}