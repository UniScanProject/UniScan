using Microsoft.Extensions.DependencyInjection;
using Shiki.ModuleManagement;

namespace UniScan.Client.Core.Module;

public class UniScanClientModuleInitializationArgs : EventArgs
{
}

public interface IUniScanClientModule : IModule<UniScanClientModuleInitializationArgs>
{
    void ConfigureDi(IServiceCollection services) {}
}