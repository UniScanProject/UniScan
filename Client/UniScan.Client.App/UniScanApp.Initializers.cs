using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using UniScan.Client.App.Core.Module;
using UniScan.Client.App.Core.Module.Modules.Internal;
using UniScan.Client.App.Core.Pipeline.Initialization;
using UniScan.Client.Core;
using UniScan.Client.Core.DI.Factory;
using UniScan.Client.Core.Remote;
using UniScan.Network;
using UniScan.Network.Registry.Source.Sources;
using UniScan.Platform.DependencyInjection;

namespace UniScan.Client.App;

public partial class UniScanApp
{
    internal async Task InitializeEnvironment(UniScanAppInitializationPipeline.TaskContexts.Early context, CancellationToken ct = default)
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

        context.ServiceCollection.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });
        Log.Debug("Initialized Environment {Env}", _hostEnvironment);
    }

    internal async Task InitializeSoftwareInfo(UniScanAppInitializationPipeline.TaskContexts.Early ctx, CancellationToken ct = default)
    {
        ctx.Status.Value = "Initializing SoftwareInfo";

        ctx.ServiceCollection.AddSingleton(SoftwareInfo);
        Log.Information("{Info}", SoftwareInfo);
    }

    internal async Task InitializeModules(UniScanAppInitializationPipeline.TaskContexts.Early ctx, CancellationToken ct = default)
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
            ctx.ServiceCollection.AddSingleton(module);
        }
    }

    internal async Task InitializeClient(UniScanAppInitializationPipeline.TaskContexts.PreClient ctx, CancellationToken ct = default)
    {
        ctx.Status.Value = "Initializing client";
        ctx.ServiceCollection.AddUniScanClient(_hostEnvironment, SoftwareInfo);
    }

    internal Task FinishInitialization(UniScanAppInitializationPipeline.TaskContexts.PostServiceProvider ctx, CancellationToken ct = default)
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