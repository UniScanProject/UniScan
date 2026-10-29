#if UNISCAN_SERVER

using Shiki.Common.Identity;
using UniScan.Server.Core;
using UniScan.Server.Core.Module;

namespace UniScan.Modules.LibUvNetworkGroup;

public class LibUvNetworkGroupServerModule : LibUvNetworkGroupModule, IUniScanServerModule
{
    public override Identifier Id { get; } = UniScanServer.Identifier.Derived("modules", "lib_uv_network_group");
}
#endif