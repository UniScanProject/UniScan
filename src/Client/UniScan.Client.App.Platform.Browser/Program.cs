using Avalonia;
using Avalonia.Browser;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Result;
using Shiki.FileSystem.Abstractions;
using Shiki.FileSystem.Implementations.Web;
using Shiki.FileSystem.Implementations.Web.Stream;
using Shiki.TaskPipeline;
using SpawnDev;
using SpawnDev.SpawnJS;
using SpawnDev.SpawnJS.WebWorkers;
using UniScan.Client.App.Core.Pipeline.Initialization;
using UniScan.Client.App.Platform.Browser.Interop;
using UniScan.Core;
using UniScan.Platform;
using UniScan.Platform.Implementations.Web;

namespace UniScan.Client.App.Platform.Browser;

//TODO when possible create special browser module, then add warning dialog if OPFS is not available.
internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        await JSMethods.Initialize();
        Dispatcher.UIThread.UnhandledException += (sender, e) =>
        {
            JSMethods.OnExit(-1, e.Exception.ToString());
        };

        UniScanApp.InitializationPipelineCreated += pipeline =>
        {
            pipeline.PipelineBuilder.TryInsertBefore<UniScanAppInitializationPipeline.TaskContexts.PreEnvironment>(UniScanAppInitializationPipeline.Initializers.InitializeEnvironment, async Task (ctx) =>
            {
                IServiceCollection browserServices = new ServiceCollection();
                
                browserServices.AddSpawnJSRuntime(out SpawnJSRuntime runtime);
                browserServices.AddWebWorkerService(webWorkerService =>
                {
                    webWorkerService.TaskPool.MaxPoolSize = -1;
                    webWorkerService.TaskPool.PoolSize = webWorkerService.GlobalScope == GlobalScope.Window ? 2 : 0;
                });

                browserServices.AddSingleton<BrowserDirectoryManager>();
                browserServices.AddSingleton<BrowserFileManager>();
                
                browserServices.AddSingleton<IOPFSWorkerService, OPFSWorkerService>();
                IServiceProvider browserServiceProvider = browserServices.BuildServiceProvider();
                await browserServiceProvider.StartBackgroundServices();
                
                ctx.StandardPaths = new BrowserPaths();
                ctx.SerilogInitializer = new BrowserPlatformSerilogInitializer(Constants.ConsoleOutputTemplate);
                ctx.FileManager = browserServiceProvider.GetRequiredService<BrowserFileManager>();
                ctx.DirectoryManager = browserServiceProvider.GetRequiredService<BrowserDirectoryManager>();
                
                ctx.ServiceCollection.AddSingleton(runtime);
                ctx.ServiceCollection.AddSingleton<WebWorkerService>(_ => browserServiceProvider.GetRequiredService<WebWorkerService>());
                
                ctx.ServiceCollection.AddSingleton<IPlatformStandardPaths>(_ => ctx.StandardPaths);
                ctx.ServiceCollection.AddSingleton<IPlatformSerilogInitializer>(_ => ctx.SerilogInitializer);
                ctx.ServiceCollection.AddSingleton<IPlatformDirectoryManager>(_ => ctx.DirectoryManager);
                ctx.ServiceCollection.AddSingleton<IPlatformFileManager>(_ => ctx.FileManager);
                ctx.ServiceCollection.AddSingleton<IOPFSWorkerService>(_ => browserServiceProvider.GetRequiredService<IOPFSWorkerService>());
            }).ThrowIfError();
            
            return Task.CompletedTask;
        };
        
        await BuildAvaloniaApp()
           .WithInterFont()
#if DEBUG
           .WithDeveloperTools()
#endif
           .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<UniScanApp>();
}