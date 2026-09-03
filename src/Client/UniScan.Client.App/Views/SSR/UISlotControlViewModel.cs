using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using Serilog;
using Shiki.Common.Identity;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Client.App.Views.SSR;

public interface IUISlotControlViewModel
{
    Identifier Identifier { get; }
    object? Node { get; set; }
}

public partial class UISlotControlViewModel : ViewModelBase, IUISlotControlViewModel
{
    public UISlotControlViewModel(Identifier identifier)
    {
        Identifier = identifier;
    }

    public Identifier Identifier { get; }

    [ObservableProperty]
    public partial object? Node { get; set; }
}