#if UNISCAN_APP
using Shiki.Common.Identity;
using UniScan.Client.App;
using UniScan.Client.App.Core.Module;

namespace UniScan.Modules.WebSocketTransportMethod;

public class WebSocketTransportMethodAppModule : WebSocketTransportMethodModule,
                                                 IUniScanClientAppModule
{
    public override Identifier Id { get; } = UniScanApp.Identifier.Derived("modules", "websocket_transport_method");
}

#endif