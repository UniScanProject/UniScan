using Microsoft.Extensions.DependencyInjection;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Module;

namespace UniScan.Client.Core.Module;

public class UniScanClientModuleInitializationArgs : EventArgs
{
}

public interface IUniScanClientModule : IModule
{
    void ConfigureDi(IServiceCollection services) {}
}