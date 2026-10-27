using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Result;
using Shiki.FileSystem.Abstractions;
using Shiki.FileSystem.Implementations.Native;
using UniScan.Client.App.Core.Pipeline.Initialization;
using UniScan.Client.App.Platform.Android.Platform;
using UniScan.Platform;
using UniScan.Platform.Implementations.Native;

namespace UniScan.Client.App.Platform.Android;

[Application]
public class Application : AvaloniaAndroidApplication<UniScanApp>
{
    protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        UniScanApp.InitializationPipelineCreated += pipeline =>
        {
            pipeline.PipelineBuilder.TryInsertBefore<UniScanAppInitializationPipeline.TaskContexts.PreEnvironment>(UniScanAppInitializationPipeline.Initializers.InitializeEnvironment, (ctx) =>
            {
                ctx.StandardPaths = new AndroidPaths();
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
        return base.CustomizeAppBuilder(builder)
                   .WithInterFont();
    }
}