using Shiki.Common.Identity;
using Shiki.ModuleManagement.Module;

namespace UniScan.Modules.WebSocketTransportMethod;

public abstract class WebSocketTransportMethodModule : IModule
{
    public abstract Identifier Id { get; }
}