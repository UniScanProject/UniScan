using Shiki.Common.Identity;
using Shiki.ModuleManagement.Module;

namespace UniScan.Modules.LibUvNetworkGroup;

public abstract class LibUvNetworkGroupModule : IModule
{
    public abstract Identifier Id { get; }
}