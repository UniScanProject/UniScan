#if UNISCAN_CLIENT

using Shiki.Common.Identity;
using UniScan.Client.Core;
using UniScan.Client.Core.Module;

namespace UniScan.Modules.LibUvNetworkGroup;

public class LibUvNetworkGroupClientModule : LibUvNetworkGroupModule, IUniScanClientModule
{
    public override Identifier Id { get; } = UniScanClient.Identifier.Derived("modules", "lib_uv_network_group");
}

#endif