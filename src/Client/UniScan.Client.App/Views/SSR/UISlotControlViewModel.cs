using CommunityToolkit.Mvvm.ComponentModel;
using Shiki.Common.Identity;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Client.App.Views.SSR;

public interface IUISlotControlViewModel
{
    Identifier Identifier { get; }
    ObservableObject? Content { get; set; }
}

public partial class UISlotControlViewModel(Identifier identifier) : ViewModelBase, IUISlotControlViewModel
{
    public Identifier Identifier { get; } = identifier;
    
    [ObservableProperty]
    public partial ObservableObject? Content { get; set; }
}