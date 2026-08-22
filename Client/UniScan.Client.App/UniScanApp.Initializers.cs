using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using UniScan.Client.App.Initialization;
using UniScan.Client.App.Module;
using UniScan.Client.App.Module.Modules.Internal;
using UniScan.Client.Core;
using UniScan.Client.Core.DI.Factory;
using UniScan.Platform.DependencyInjection;

namespace UniScan.Client.App;

public partial class UniScanApp
{
    internal async Task InitializeEnvironment(UniScanAppInitializationPipeline.TaskContexts.Early context)
    {
        context.Status.Value = "Initializing environment";

        if (InitializePlatform == null)
            throw new ArgumentException("No platform initializer was provided");

        _hostEnvironment = await InitializePlatform();

        if (_hostEnvironment == null)
            throw new ArgumentException("Platform not initialized");

        await _hostEnvironment.StandardPaths.CreateAllAsync(_hostEnvironment.DirectoryManager);
        Log.Logger = _hostEnvironment.SerilogInitializer.GetConfiguration(_hostEnvironment).CreateLogger()
                                     .ForContext<UniScanApp>();

        _hostEnvironment.AddToDi(context.ServiceCollection);
        Log.Logger.Debug("Initialized Environment {Env}", _hostEnvironment);
    }

    internal async Task InitializeSoftwareInfo(UniScanAppInitializationPipeline.TaskContexts.Early ctx)
    {
        ctx.Status.Value = "Initializing SoftwareInfo";

        ctx.ServiceCollection.AddSingleton(SoftwareInfo);
        Log.Information("{Info}", SoftwareInfo);
    }

    internal async Task InitializeModules(UniScanAppInitializationPipeline.TaskContexts.Early ctx)
    {
        ctx.Status.Value = "Initializing modules";

        ModuleStorage = new ModuleStorage<IUniScanClientAppModule, UniScanClientAppModuleInitializationArgs>()
           .WithModulesFrom(new TypeListModuleSource(typeof(InternalUniScanClientAppModule)),
                            new UniScanClientAppModuleInitializationArgs(_hostEnvironment));

        string moduleDir = Path.Combine(_hostEnvironment.StandardPaths.DataPath, "modules");
        if (!(await _hostEnvironment.DirectoryManager.ExistsAsync(moduleDir)))
        {
            Log.Information("Creating new modules folder");
            await _hostEnvironment.DirectoryManager.CreateDirectoryAsync(moduleDir);
        }

        try
        {
            ModuleStorage.LoadFrom(new AssembliesModuleSource(moduleDir),
                                   new UniScanClientAppModuleInitializationArgs(_hostEnvironment));
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            Log.Error(ex, "Failed to load assembly modules");
        }

        foreach (IUniScanClientAppModule module in ModuleStorage.Modules)
        {
            module.ConfigureDi(ctx.ServiceCollection);
        }
    }

    internal async Task InitializeClient(UniScanAppInitializationPipeline.TaskContexts.PreClient ctx)
    {
        ctx.Status.Value = "Initializing client and loading remotes";

        ctx.Client = await UniScanClient.CreateInstanceAsync(_hostEnvironment, SoftwareInfo);
        ctx.ServiceCollection.AddSingleton(ctx.Client);
        ctx.ServiceCollection.AddSingleton<IRemoteFactory>(_ => ctx.Client.ServiceProvider.GetRequiredService<IRemoteFactory>());
    }

    internal Task FinishInitialization(UniScanAppInitializationPipeline.TaskContexts.PostServiceProvider ctx)
    {
        try
        {
            ctx.Status.Value = "Finishing up";

            ServiceProvider = ctx.Services;
            LoadingComplete?.Invoke();
            
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }
}