using System;
using System.Threading.Tasks;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Result;
using Shiki.FileSystem.Abstractions;
using Shiki.FileSystem.Implementations.Native;
using UniScan.Client.App.Core.Pipeline.Initialization;
using UniScan.Platform;
using UniScan.Platform.Implementations.Native;

namespace UniScan.Client.App.Platform.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        UniScanApp.InitializationPipelineCreated += pipeline =>
        {
            pipeline.PipelineBuilder.TryInsertBefore<UniScanAppInitializationPipeline.TaskContexts.PreEnvironment>(UniScanAppInitializationPipeline.Initializers.InitializeEnvironment, (ctx) =>
            {
                 ctx.StandardPaths = new DesktopPaths();
                 ctx.SerilogInitializer = new NativePlatformSerilogInitializer(UniScan.Core.Constants.ConsoleOutputTemplate, UniScan.Core.Constants.FileOutputTemplate, ctx.StandardPaths);
                 ctx.FileManager = NativeFileManager.Instance;
                 ctx.DirectoryManager = NativeDirectoryManager.Instance;
                 
                 ctx.ServiceCollection.AddSingleton<IPlatformStandardPaths>(_ => ctx.StandardPaths);
                 ctx.ServiceCollection.AddSingleton<IPlatformSerilogInitializer>(_ => ctx.SerilogInitializer);
                 ctx.ServiceCollection.AddSingleton<IPlatformDirectoryManager>(_ => ctx.DirectoryManager);
                 ctx.ServiceCollection.AddSingleton<IPlatformFileManager>(_ => ctx.FileManager);
            }).ThrowIfError();

            return Task.CompletedTask;
        };
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<UniScanApp>()
                     .UsePlatformDetect()
#if DEBUG
                     .WithDeveloperTools()
#endif
                     .WithInterFont()
                     .LogToTrace();
}