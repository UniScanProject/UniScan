#if UNISCAN_CLIENT && UNISCAN_APP
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using UniScan.Client.App.UI.ConnectionMethod;
using UniScan.Client.App.UI.Validation;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Modules.WebSocketTransportMethod.Network;
using UniScan.Modules.WebSocketTransportMethod.Parser;

namespace UniScan.Modules.WebSocketTransportMethod.View;

[ConnectionMethodFactoryViewModel("WebSocket")]
public partial class WebSocketConnectionMethodFactoryViewModel : ViewModelBase, IConnectionMethodFactoryViewModel<WebSocketConnectionMethodFactoryViewModel, WebSocketRemoteConnectionMethod>
{
    public WebSocketConnectionMethodFactoryViewModel()
    {
        ValidateAllProperties();
    }
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "URL is required")]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [ValidatorParser<UriValidatorParser>(ErrorMessage = "Must be a valid URL")]
    public partial string Url { get; set; }

    public bool IsValid => !GetErrors(nameof(Url)).Any();
    
    public WebSocketRemoteConnectionMethod Create() => new(new Uri(Url));
}
#endif