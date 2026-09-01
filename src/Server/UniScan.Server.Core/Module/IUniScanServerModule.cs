using Microsoft.Extensions.DependencyInjection;
using Shiki.ModuleManagement;

namespace UniScan.Server.Core.Module;

public class UniScanServerModuleInitializationArgs : EventArgs
{
    public UniScanServer ServerInstance { get; }
}

public interface IUniScanServerModule : IModule<UniScanServerModuleInitializationArgs>
{
    void ConfigureDi(IServiceCollection services) {}
}