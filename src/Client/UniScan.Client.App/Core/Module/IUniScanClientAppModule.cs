using System;
using Microsoft.Extensions.DependencyInjection;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Module;
using UniScan.Core.Settings;
using UniScan.Platform;

namespace UniScan.Client.App.Core.Module;

public class UniScanClientAppModuleInitializationArgs(UniScanApp app, SettingsManagerBuilder settingsBuilder) : EventArgs
{
    public UniScanApp App { get; }
    public SettingsManagerBuilder SettingsBuilder { get; } = settingsBuilder;
}

public interface IUniScanClientAppModule : IModule
{
    void ConfigureDi(IServiceCollection services) {}
}