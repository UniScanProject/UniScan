using System;
using Microsoft.Extensions.DependencyInjection;
using Shiki.FileSystem.Abstractions;
using Shiki.Internationalization.Translation;
using Shiki.TaskPipeline.Context;
using UniScan.Core.Settings;
using UniScan.Platform;

namespace UniScan.Client.App.Core.Pipeline.Initialization;

public partial class UniScanAppInitializationPipeline
{
    public static partial class TaskContexts
    {
        public record PreEnvironment(UniScanApp UniScanApp) : ITaskContext
        {
            public ServiceCollection ServiceCollection { get; } = new();
            
            public IPlatformStandardPaths? StandardPaths { get; set; }
            public IPlatformFileManager? FileManager { get; set; }
            public IPlatformDirectoryManager? DirectoryManager { get; set; }
            public IPlatformSerilogInitializer? SerilogInitializer { get; set; }
        }
        
        public record PreSettings(UniScanApp UniScanApp, ServiceCollection ServiceCollection, IPlatformStandardPaths PlatformStandardPaths, IPlatformFileManager PlatformFileManager, IPlatformDirectoryManager PlatformDirectoryManager)
            : ITransitionableTaskContext<PreSettings, PreEnvironment>
        {
            public SettingsManagerBuilder SettingsBuilder { get; } = new SettingsManagerBuilder();
            public SettingsManager? SettingsManager { get; set; }

            public static PreSettings TransitionFrom(PreEnvironment oldContext) => new(oldContext.UniScanApp, oldContext.ServiceCollection, oldContext.StandardPaths ?? throw new NullReferenceException(), oldContext.FileManager ?? throw new NullReferenceException(), oldContext.DirectoryManager ?? throw new NullReferenceException());
        }

        public record PreTranslations(UniScanApp UniScanApp, ServiceCollection ServiceCollection, IPlatformStandardPaths PlatformStandardPaths, IPlatformFileManager PlatformFileManager, IPlatformDirectoryManager PlatformDirectoryManager, SettingsManager SettingsManager)
            : ITransitionableTaskContext<PreTranslations, PreSettings>
        {
            public TranslationManager? TranslationManager { get; set; }

            public static PreTranslations TransitionFrom(PreSettings oldContext) => new(oldContext.UniScanApp, oldContext.ServiceCollection, oldContext.PlatformStandardPaths, oldContext.PlatformFileManager, oldContext.PlatformDirectoryManager, oldContext.SettingsManager ?? throw new NullReferenceException());
        }

        public record PreServiceProvider(
            UniScanApp UniScanApp,
            ServiceCollection ServiceCollection,
            TranslationManager TranslationManager,
            IPlatformStandardPaths PlatformStandardPaths,
            IPlatformFileManager PlatformFileManager,
            IPlatformDirectoryManager PlatformDirectoryManager,
            SettingsManager SettingsManager)
            : ITaskContext;

        public record UniScanAppReady(UniScanApp UniScanApp, SettingsManager SettingsManager, IServiceProvider ServiceProvider) : ITaskContext;
    }
}