using System;
using Microsoft.Extensions.DependencyInjection;
using Shiki.FileSystem.Abstractions;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Module;
using Shiki.Settings;
using UniScan.Platform;

namespace UniScan.Client.App.Core.Module;

public readonly record struct UniScanClientAppModuleInitializationArgs(
    UniScanApp App,
    SettingsManagerBuilder SettingsBuilder, IPlatformStandardPaths PlatformStandardPaths, IPlatformFileManager FileManager, IPlatformDirectoryManager DirectoryManager);

public interface IUniScanClientAppModule : IModule
{
    void ConfigureDi(IServiceCollection services) {}
}