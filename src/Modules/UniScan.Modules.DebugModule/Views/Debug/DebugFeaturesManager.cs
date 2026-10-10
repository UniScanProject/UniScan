using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using UniScan.Client.App.UI.ServersideRendering.Binding;
using UniScan.Client.App.Views;

namespace UniScan.Modules.DebugModule.Views.Debug;

public partial class DebugFeaturesManager(IUIBindingsRegistry registry, MainViewModel mvm) : IDisposable
{
    private IDisposable? _keybindsHandler;

    public IUIBindingsRegistry Registry { get; } = registry;
    public MainViewModel MainViewModel { get; } = mvm;

    public StateDebugPageViewModel DebugPageViewModel { get; } = new(registry as UIBindingsRegistry);
    
    public void Initialize()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _keybindsHandler = desktop.MainWindow?.AddDisposableHandler(InputElement.KeyDownEvent, OnKeyDown);
        }
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e is { Key: Key.D, KeyModifiers: KeyModifiers.Control })
        {
            DebugFeaturesDialog dialog = new()
            {
                DataContext = this
            };

            await DialogHostAvalonia.DialogHost.Show(dialog, "MainDialogHost");
        }
    }

    [RelayCommand]
    public void CloseDialog()
    {
        DialogHostAvalonia.DialogHost.Close("MainDialogHost");
    }

    public void Dispose()
    {
        _keybindsHandler?.Dispose();
    }
}