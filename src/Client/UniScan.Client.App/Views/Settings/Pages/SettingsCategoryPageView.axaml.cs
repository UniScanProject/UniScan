using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using UniScan.Client.App.UI.Conversion;
using UniScan.Client.App.UI.Conversion.Options;

namespace UniScan.Client.App.Views.Settings.Pages;

public partial class SettingsCategoryPageView : UserControl
{
    public SettingsCategoryPageView()
    {
        InitializeComponent();

        OptionItemsControl.ItemTemplate = UniScanApp.ServiceProvider.GetRequiredService<OptionDataTemplate>();
    }
}