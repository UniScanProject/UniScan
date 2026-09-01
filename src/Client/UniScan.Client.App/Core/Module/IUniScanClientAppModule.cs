using System;
using Microsoft.Extensions.DependencyInjection;
using Shiki.ModuleManagement;
using UniScan.Platform;

namespace UniScan.Client.App.Core.Module;

public class UniScanClientAppModuleInitializationArgs(HostEnvironment hostEnvironment) : EventArgs
{
    public HostEnvironment HostEnvironment { get; } = hostEnvironment;
}

public interface IUniScanClientAppModule : IModule<UniScanClientAppModuleInitializationArgs>
{
    void ConfigureDi(IServiceCollection services) {}
}