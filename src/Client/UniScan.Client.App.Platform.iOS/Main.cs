using UIKit;
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

namespace UniScan.Client.App.Platform.iOS;

public class Application
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        UniScanApp.InitializationPipelineCreated += pipeline =>
        {
            pipeline.PipelineBuilder.TryInsertBefore<UniScanAppInitializationPipeline.TaskContexts.PreEnvironment>(UniScanAppInitializationPipeline.Initializers.InitializeEnvironment, (ctx) =>
            {
                ctx.StandardPaths = new iOSPaths();
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
        
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}