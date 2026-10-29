using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Shiki.Common.Result;
using Shiki.FileSystem.Abstractions;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using Shiki.TaskPipeline;
using Shiki.TaskPipeline.Context;
using UniScan.Client.App.Core.Module;
using UniScan.Client.App.Core.Module.Modules.Internal;
using UniScan.Client.App.Core.Pipeline.Initialization;
using UniScan.Client.App.UI.Conversion;
using UniScan.Client.App.UI.Conversion.Options;
using UniScan.Client.Core;
using Shiki.Settings;
using UniScan.Platform;

namespace UniScan.Client.App;

public partial class UniScanApp
{
    internal async Task InitializeSettings(UniScanAppInitializationPipeline.TaskContexts.PreSettings ctx,
                                           ITaskExecutionContext execCtx)
    {
        execCtx.Status.Value = "Loading settings";
        
        var res = await ctx.SettingsBuilder.BuildAndLoadAsync();
        
        var errors = res.Results.Where(r => !r.IsSuccessful).ToArray();
        if (errors.Length != 0)
        {

            StringBuilder b = new();
            foreach (var r in errors)
            {
                b.Append($"- {r.Error!.ToString()}\n");
            }
            
            Log.Error("Errors were encountered during settings load: \n{Errors}", b);
        }

        ctx.SettingsManager = res.settingsManager;
        ctx.ServiceCollection.AddSingleton(_ => res.settingsManager);
        ctx.ServiceCollection.AddSingleton<OptionDataTemplate>();
    }
    
    
    internal async Task InitializeModules(UniScanAppInitializationPipeline.TaskContexts.PreSettings ctx, ITaskExecutionContext execCtx)
    {
        execCtx.Status.Value = "Initializing modules";

        UniScanClientAppModuleInitializationArgs args = new(this, ctx.SettingsBuilder, ctx.PlatformStandardPaths, ctx.PlatformFileManager, ctx.PlatformDirectoryManager);
        
        var moduleStorageBuilder = new ModuleStorageBuilder<IUniScanClientAppModule, UniScanClientAppModuleInitializationArgs>()
           .WithModuleOfType<InternalUniScanClientAppModule>(args);
        
        string moduleDir = Path.Combine(ctx.PlatformStandardPaths.DataPath, "modules");
        if (!(await ctx.PlatformDirectoryManager.ExistsAsync(moduleDir)))
        {
            Log.Information("Creating new modules folder");
            await ctx.PlatformDirectoryManager.CreateDirectoryAsync(moduleDir);
        }
        
        moduleStorageBuilder.WithSource(new AssembliesModuleSource(moduleDir), args);

        string localModules = Path.Combine(Environment.CurrentDirectory, "modules");
        if (await ctx.PlatformDirectoryManager.ExistsAsync(localModules))
        {
            moduleStorageBuilder.WithSource(new AssembliesModuleSource(localModules), args);
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