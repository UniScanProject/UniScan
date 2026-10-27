using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Serilog;
using Shiki.FileSystem.Implementations.Assembly;
using Shiki.FileSystem.Implementations.Native;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using Shiki.TaskPipeline.Context;
using UniScan.Core.Serialization;
using UniScan.Server.Core.Module;
using UniScan.Server.Core.Module.Modules.Internal;

namespace UniScan.Server.Host.Pipeline;

public partial class UniScanServerHostInitializationPipeline
{
    public static class Initializers
    {
        public static async Task InitializeEnvironment(TaskContexts.PreEnvironment ctx, ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing environment";
            
            ctx.FileManager = NativeFileManager.Instance;
            ctx.DirectoryManager = NativeDirectoryManager.Instance;
            
            string devicesDirectory = Path.Combine(ctx.RootDirectory, "devices");
            if (!await ctx.DirectoryManager.ExistsAsync(devicesDirectory))
                await ctx.DirectoryManager.CreateDirectoryAsync(devicesDirectory);
        
            string optionsDirectory = Path.Combine(ctx.RootDirectory, "settings");
            if (!await ctx.DirectoryManager.ExistsAsync(optionsDirectory))
                await ctx.DirectoryManager.CreateDirectoryAsync(optionsDirectory);
        }
        
        public static async Task InitializeModules(TaskContexts.PreModules ctx, ITaskExecutionContext execCtx)
        {
            execCtx.Status.Value = "Initializing modules";

            UniScanServerModuleInitializationArgs args = new(ctx.Server, ctx.SettingsBuilder, ctx.FileManager, ctx.DirectoryManager);
            var builder = new ModuleStorageBuilder<IUniScanServerModule, UniScanServerModuleInitializationArgs>()
               .WithModuleOfType<InternalUniScanServerModule>(args);
            
            string moduleDir = Path.Combine(ctx.RootDirectory, "modules");
            if (!await ctx.DirectoryManager.ExistsAsync(moduleDir))
            {
                Log.Information("Creating module directory");
                await ctx.DirectoryManager.CreateDirectoryAsync(moduleDir);
            }

            builder.WithSource(new AssembliesModuleSource(moduleDir), args);
            ctx.ModuleStorage = await builder.BuildAsync();
            
            foreach (var mod in ctx.ModuleStorage.Modules)
            {
                Log.Information("Loaded module '{Id}' ({Type})", mod.Key, mod.Value.GetType().FullName);
            }
        }
    }
}