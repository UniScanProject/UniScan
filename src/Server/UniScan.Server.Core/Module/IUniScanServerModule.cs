using Microsoft.Extensions.DependencyInjection;
using Shiki.FileSystem.Abstractions;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Module;
using Shiki.Settings;

namespace UniScan.Server.Core.Module;

public record UniScanServerModuleInitializationArgs(
    UniScanServer ServerInstance,
    SettingsManagerBuilder SettingsBuilder,
    IPlatformFileManager FileManager,
    IPlatformDirectoryManager DirectoryManager);

public interface IUniScanServerModule : IModule
{
    void ConfigureDi(IServiceCollection services) {}
}