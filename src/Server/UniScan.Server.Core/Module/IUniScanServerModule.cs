using Microsoft.Extensions.DependencyInjection;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Module;

namespace UniScan.Server.Core.Module;

public class UniScanServerModuleInitializationArgs : EventArgs
{
    public UniScanServer ServerInstance { get; }
}

public interface IUniScanServerModule : IModule
{
    void ConfigureDi(IServiceCollection services) {}
}