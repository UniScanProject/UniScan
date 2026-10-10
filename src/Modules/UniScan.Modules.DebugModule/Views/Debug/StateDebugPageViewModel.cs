using UniScan.Client.App.UI.ServersideRendering.Binding;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Modules.DebugModule.Views.Debug;

public class StateDebugPageViewModel(UIBindingsRegistry bindingsRegistry) : ViewModelBase
{
    public UIBindingsRegistry BindingsRegistry { get; } = bindingsRegistry;
}