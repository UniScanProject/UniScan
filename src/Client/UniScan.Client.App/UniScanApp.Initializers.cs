using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Shiki.FileSystem.Abstractions;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using Shiki.TaskPipeline;
using Shiki.TaskPipeline.Context;
using UniScan.Client.App.Core.Module;
using UniScan.Client.App.Core.Module.Modules.Internal;
using UniScan.Client.App.Core.Pipeline.Initialization;
using UniScan.Client.Core;
using UniScan.Platform;

namespace UniScan.Client.App;

public partial class UniScanApp
{
    internal async Task InitializeModules(UniScanAppInitializationPipeline.TaskContexts.PreTranslations ctx, ITaskExecutionContext execCtx)
    {
        execCtx.Status.Value = "Initializing modules";

        var moduleStorageBuilder = new ModuleStorageBuilder<IUniScanClientAppModule, UniScanClientAppModuleInitializationArgs>()
           .WithModuleOfType<InternalUniScanClientAppModule>(null);

        await using ServiceProvider serviceProvider = ctx.ServiceCollection.BuildServiceProvider();
        IPlatformStandardPaths standardPaths = serviceProvider.GetRequiredService<IPlatformStandardPaths>();
        IPlatformDirectoryManager directoryManager = serviceProvider.GetRequiredService<IPlatformDirectoryManager>();
        
        string moduleDir = Path.Combine(standardPaths.DataPath, "modules");
        if (!(await directoryManager.ExistsAsync(moduleDir)))
        {
            Log.Information("Creating new modules folder");
            await directoryManager.CreateDirectoryAsync(moduleDir);
        }
        
        moduleStorageBuilder.WithSource(new AssembliesModuleSource(moduleDir), new UniScanClientAppModuleInitializationArgs(this));

        string localModules = Path.Combine(Environment.CurrentDirectory, "modules");
        if (await directoryManager.ExistsAsync(localModules))
        {
            moduleStorageBuilder.WithSource(new AssembliesModuleSource(localModules), new UniScanClientAppModuleInitializationArgs(this));
        }

        ModuleStorage = await moduleStorageBuilder.BuildAsync();
        
        foreach (var module in ModuleStorage.Modules)
        {
            execCtx.Status.Value = $"Initializing module '{module.Key}'";
            
            module.Value.ConfigureDi(ctx.ServiceCollection);
            ctx.ServiceCollection.AddSingleton<IUniScanClientAppModule>(module.Value);
            
            Log.Information("Initialized client module '{Id}' ({TypeName})", module.Key, module.Value.Id);
        }
    }

    internal Task FinishInitialization(UniScanAppInitializationPipeline.TaskContexts.UniScanAppReady ctx, ITaskExecutionContext execCtx)
    {
        try
        {
            execCtx.Status.Value = "Finishing up";

            ServiceProvider = ctx.ServiceProvider;
            LoadingComplete?.Invoke();
            InitializationPipelineFinished?.Invoke(this);
            
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }
}