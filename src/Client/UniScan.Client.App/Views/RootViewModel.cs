using CommunityToolkit.Mvvm.ComponentModel;
using Shiki.Common.Identity;
using UniScan.Client.App.Views.ViewModel;

namespace UniScan.Client.App.Views;

public partial class RootViewModel(ObservableObject page) : GlobalSubPagedViewModelBase<RootViewModel>(page), IGlobalSubPagedViewModel
{
    public static Identifier Identifier { get; } = UniScanApp.Identifier.Derived("view_model", "root");
}