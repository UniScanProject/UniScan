using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using UniScan.Client.App.UI.ConnectionMethod;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Modules.DebugModule.Core;

namespace UniScan.Modules.DebugModule.Views.ConnectionMethod;

[ConnectionMethodFactoryViewModel("No-OP")]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.AllConstructors)]
public partial class NoOpConnectionMethodFactoryViewModel : ViewModelBase, IConnectionMethodFactoryViewModel<NoOpConnectionMethodFactoryViewModel, NoOpRemoteConnectionMethod>
{
    public NoOpConnectionMethodFactoryViewModel()
    {
        ValidateAllProperties();
    }
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is required")]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    public partial string Name { get; set; }
    
    public bool IsValid => !GetErrors(nameof(Name)).Any();
    
    public NoOpRemoteConnectionMethod Create() => new(Name);
}