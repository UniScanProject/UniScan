using System.Collections.Immutable;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Shiki.Common.Identity;
using UniScan.Client.App.Views.Global;
using UniScan.Client.App.Views.Settings.Pages;
using UniScan.Client.App.Views.ViewModel;
using Shiki.Settings;
using Shiki.Settings.Category;
using UniScan.Client.App.UI;

namespace UniScan.Client.App.Views.Settings;

public partial class ClientSettingsViewModel : GlobalSubPagedViewModelBase<ClientSettingsViewModel>, IGlobalSubPagedViewModel
{
    public new static Identifier Identifier { get; } = UniScanApp.Identifier.Derived("view_model", "settings");

    public ImmutableArray<SettingsCategoryNode> Categories { get; }
    
    public INavigationRouter<ObservableObject> Router { get; }
    
    [ObservableProperty]
    public partial SettingsCategoryNode? Current { get; set; }   
    
    [ObservableProperty]
    public partial SettingsCategoryNode? SidebarCurrent { get; set; }

    public ClientSettingsViewModel(SettingsManager manager) : base(SettingsEmptyPageViewModel.Instance)
    {
        Router = new NavigationRouter<ObservableObject>(this);
        Categories = manager.RootCategories;

        Current = Categories.FirstOrDefault();
    }

    partial void OnCurrentChanged(SettingsCategoryNode? value)
    {
        if (value is null) return;

        if (value.Children.Length > 0)
        {
            SidebarCurrent = value.Children[0];
        }
        else
        {
            SidebarCurrent = null;
            
            Router.Navigate(new SettingsCategoryPageViewModel(value, Router), true);
        }
    }

    partial void OnSidebarCurrentChanged(SettingsCategoryNode? value)
    {
        if (value is null) return;
        
        Router.Navigate(new SettingsCategoryPageViewModel(value, Router), true);
    }
}