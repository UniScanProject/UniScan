using System;
using Microsoft.Extensions.DependencyInjection;
using Shiki.FileSystem.Abstractions;
using Shiki.Internationalization.Translation;
using Shiki.TaskPipeline.Context;
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

        public record PreTranslations(UniScanApp UniScanApp, ServiceCollection ServiceCollection, IPlatformStandardPaths PlatformStandardPaths, IPlatformFileManager PlatformFileManager, IPlatformDirectoryManager PlatformDirectoryManager)
            : ITransitionableTaskContext<PreTranslations, PreEnvironment>
        {
            public TranslationManager? TranslationManager { get; set; }

            public static PreTranslations TransitionFrom(PreEnvironment oldContext) => new(oldContext.UniScanApp, oldContext.ServiceCollection, oldContext.StandardPaths ?? throw new NullReferenceException(), oldContext.FileManager ?? throw new NullReferenceException(), oldContext.DirectoryManager ?? throw new NullReferenceException());
        }

        public record PreServiceProvider(
            UniScanApp UniScanApp,
            ServiceCollection ServiceCollection,
            TranslationManager TranslationManager,
            IPlatformStandardPaths PlatformStandardPaths,
            IPlatformFileManager PlatformFileManager,
            IPlatformDirectoryManager PlatformDirectoryManager)
            : ITaskContext;

        public record UniScanAppReady(UniScanApp UniScanApp, IServiceProvider ServiceProvider) : ITaskContext;
    }
}