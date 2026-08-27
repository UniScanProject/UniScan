using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using UniScan.Client.App.UI.ConnectionMethod;
using UniScan.Client.App.Views.ViewModel;
using UniScan.Network.Client.Remote.Connection.Methods;

namespace UniScan.Client.App.Views.Remote.Connection.ConnectionMethod;

#if !browser
[ConnectionMethodFactoryViewModel("TCP")]
public partial class TCPConnectionMethodFactoryViewModel : ViewModelBase, IConnectionMethodFactoryViewModel<TCPConnectionMethodFactoryViewModel, TCPRemoteConnectionMethod>
{
    public TCPConnectionMethodFactoryViewModel()
    {
        ValidateAllProperties();
    }
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Endpoint is required")]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [IPEndpointValidator(ErrorMessage = "Must be a valid IPEndpoint containing an IP and port, such as 127.0.0.1:9000")]
    public partial string EndPoint { get; set; }
    
    public bool IsValid => !GetErrors(nameof(EndPoint)).Any();
    
    public TCPRemoteConnectionMethod Create() => new(IPEndPoint.Parse(EndPoint));
}

public class IPEndpointValidatorAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string v && IPEndPoint.TryParse(v, out _))
        {
            return ValidationResult.Success;
        }
        
        return new ValidationResult(ErrorMessage);
    }
}
#endif