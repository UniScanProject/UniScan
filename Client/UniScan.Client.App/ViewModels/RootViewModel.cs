using CommunityToolkit.Mvvm.ComponentModel;
using Shiki.Common.Identity;
using UniScan.Client.Core;

namespace UniScan.Client.App.ViewModels;

public partial class RootViewModel(ObservableObject page) : SingletonSubPagedViewModelBase<RootViewModel>(page), ISingletonSubPagedViewModel
{
    public static Identifier Identifier { get; } = UniScanApp.Identifier.Derived("view_model", "root");
}