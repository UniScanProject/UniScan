using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Shiki.FileSystem.Abstractions;
using Shiki.ModuleManagement;
using Shiki.TaskPipeline.Context;
using Shiki.Settings;
using UniScan.Platform;
using UniScan.Server.Core;
using UniScan.Server.Core.Module;

namespace UniScan.Server.Host.Pipeline;

public partial class UniScanServerHostInitializationPipeline
{
    public static partial class TaskContexts
    {
        public record PreEnvironment(UniScanServer Server, string RootDirectory) : ITaskContext
        {
            public IPlatformFileManager? FileManager { get; set; }
            public IPlatformDirectoryManager? DirectoryManager { get; set; }
        }

        public record PreModules(UniScanServer Server, string RootDirectory, IPlatformFileManager FileManager, IPlatformDirectoryManager DirectoryManager) : ITransitionableTaskContext<PreModules, PreEnvironment>
        {
            public SettingsManagerBuilder SettingsBuilder { get; } = new();
            public ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs>? ModuleStorage { get; set; }
            
            public static PreModules TransitionFrom(PreEnvironment oldContext) => new(oldContext.Server, oldContext.RootDirectory, oldContext.FileManager ?? throw new NullReferenceException(), oldContext.DirectoryManager ?? throw new NullReferenceException());
        }
        
        public record HostReady(UniScanServer Server, string RootDirectory, IPlatformFileManager FileManager, IPlatformDirectoryManager DirectoryManager, SettingsManagerBuilder SettingsBuilder, ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs> ModuleStorage) : ITransitionableTaskContext<HostReady, PreModules>
        {
            public static HostReady TransitionFrom(PreModules oldContext) => new(oldContext.Server, oldContext.RootDirectory, oldContext.FileManager, oldContext.DirectoryManager, oldContext.SettingsBuilder, oldContext.ModuleStorage ?? throw new NullReferenceException());
        }
    }
}