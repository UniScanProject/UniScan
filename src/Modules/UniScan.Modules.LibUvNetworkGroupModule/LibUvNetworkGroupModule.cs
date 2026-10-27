using Shiki.Common.Identity;
using UniScan.Client.Core.Module;
using UniScan.Server.Core.Module;

namespace UniScan.Modules.LibUvNetworkGroup;

public class LibUvNetworkGroupModule : IUniScanClientModule, IUniScanServerModule
{
    public Identifier Id { get; } = new("UniScan", "modules", "lib_uv_network_group");
}