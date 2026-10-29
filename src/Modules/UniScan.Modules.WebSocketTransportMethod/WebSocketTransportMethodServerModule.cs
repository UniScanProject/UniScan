#if UNISCAN_SERVER

using Shiki.Common.Identity;
using UniScan.Server.Core;
using UniScan.Server.Core.Module;

namespace UniScan.Modules.WebSocketTransportMethod;

public class WebSocketTransportMethodServerModule : WebSocketTransportMethodModule, IUniScanServerModule
{
    public override Identifier Id { get; } = UniScanServer.Identifier.Derived("modules", "websocket_transport_method");

}
#endif