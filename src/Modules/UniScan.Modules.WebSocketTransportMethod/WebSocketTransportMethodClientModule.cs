#if UNISCAN_CLIENT

using Shiki.Common.Identity;
using UniScan.Client.Core;
using UniScan.Client.Core.Module;

namespace UniScan.Modules.WebSocketTransportMethod;

public class WebSocketTransportMethodClientModule : WebSocketTransportMethodModule, IUniScanClientModule
{
    public override Identifier Id { get; } = UniScanClient.Identifier.Derived("modules", "websocket_transport_method");
}

#endif