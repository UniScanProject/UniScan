using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Shiki.FileSystem.Abstractions;
using Shiki.ModuleManagement;
using Shiki.TaskPipeline.Context;
using Shiki.Settings;
using UniScan.Server.Core.Module;

namespace UniScan.Server.Core.Pipeline;

public partial class UniScanServerInitializationPipeline
{
    public static partial class TaskContexts
    {
        public record PreEnvironment(
            UniScanServer Server,
            string RootDirectory,
            IPlatformFileManager FileManager,
            IPlatformDirectoryManager DirectoryManager,
            ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs> ModuleStorage,
            SettingsManagerBuilder SettingsBuilder) : ITaskContext
        {
            public ServiceCollection ServiceCollection { get; } = new();
        }
        
        public record PreSettings(UniScanServer Server,
                                  ServiceCollection ServiceCollection,
                                  string RootDirectory,
                                  IPlatformFileManager FileManager,
                                  IPlatformDirectoryManager DirectoryManager,
                                  ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs> ModuleStorage,
                                  SettingsManagerBuilder SettingsBuilder)
            : ITransitionableTaskContext<PreSettings, PreEnvironment>
        {
            public SettingsManager? SettingsManager { get; set; }
            
            public static PreSettings TransitionFrom(PreEnvironment oldContext) => new(oldContext.Server, oldContext.ServiceCollection, oldContext.RootDirectory, oldContext.FileManager, oldContext.DirectoryManager, oldContext.ModuleStorage, oldContext.SettingsBuilder);
        }

        public record PreServiceProvider(
            UniScanServer Server,
            ServiceCollection ServiceCollection,
            string RootDirectory,
            IPlatformFileManager FileManager,
            IPlatformDirectoryManager DirectoryManager,
            ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs> ModuleStorage,
            SettingsManager SettingsManager
        )
            : ITransitionableTaskContext<PreServiceProvider, PreSettings>
        {
            public static PreServiceProvider TransitionFrom(PreSettings oldContext) => new(oldContext.Server, oldContext.ServiceCollection, oldContext.RootDirectory, oldContext.FileManager, oldContext.DirectoryManager, oldContext.ModuleStorage, oldContext.SettingsManager ?? throw new NullReferenceException());
        }

        public record ServerReady(UniScanServer UniScanApp, IServiceProvider ServiceProvider) : ITaskContext;
    }
}