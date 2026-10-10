using System;
using Microsoft.Extensions.DependencyInjection;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Module;
using UniScan.Platform;

namespace UniScan.Client.App.Core.Module;

public class UniScanClientAppModuleInitializationArgs(UniScanApp app) : EventArgs
{
    public UniScanApp App { get; }
}

public interface IUniScanClientAppModule : IModule
{
    void ConfigureDi(IServiceCollection services) {}
}