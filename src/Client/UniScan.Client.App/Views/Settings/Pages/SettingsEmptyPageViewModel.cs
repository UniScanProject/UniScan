using Shiki.Common.Util;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Client.App.Views.Settings.Pages;

public class SettingsEmptyPageViewModel : ViewModelBase, ISingleton<SettingsEmptyPageViewModel>
{
    public static SettingsEmptyPageViewModel Instance { get; } = new();

    private SettingsEmptyPageViewModel() {}
}