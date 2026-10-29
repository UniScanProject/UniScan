using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using UniScan.Client.App.Core;
using UniScan.Client.App.UI.ConnectionMethod;
using UniScan.Client.App.UI.Validation;
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
    [Required(ErrorMessage = "IP Address is required")]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [ValidatorParser<EndpointValidatorParser>(ErrorMessage = "Must be a valid IP Address, such as 127.0.0.1")]
    public partial string Address { get; set; }
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Port is required")]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [ParsableValidator<ushort>(ErrorMessage = "Must be a valid port number, such as 9000")]
    public partial string Port { get; set; }
    
    public bool IsValid => !GetErrors(nameof(Address)).Any() && !GetErrors(nameof(Port)).Any();
    
    public TCPRemoteConnectionMethod Create()
    {
        ushort port = ushort.Parse(Port);
        EndPoint e = IPAddress.TryParse(Address, out IPAddress? addr)
                         ? new IPEndPoint(addr, port)
                         : new DnsEndPoint(Address, port);
        
        return new TCPRemoteConnectionMethod(e);
    }
}
#endif