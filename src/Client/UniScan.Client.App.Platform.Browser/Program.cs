using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Threading;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SpawnDev;
using SpawnDev.SpawnJS;
using SpawnDev.SpawnJS.WebWorkers;
using UniScan.Client.App.Platform.Browser.Interop;
using UniScan.Client.Core;
using UniScan.Core;
using UniScan.Platform;
using UniScan.Platform.Implementations.Web;
using UniScan.Platform.Implementations.Web.Filesystem;
using UniScan.Platform.Implementations.Web.Filesystem.Stream;

namespace UniScan.Client.App.Platform.Browser;

internal sealed partial class Program
{
    public static IServiceProvider ServiceProvider { get; private set; }
    
    private static async Task Main(string[] args)
    {
        await JSMethods.Initialize();
        Dispatcher.UIThread.UnhandledException += (sender, e) =>
        {
            JSMethods.OnExit(-1, e.Exception.ToString());
        };
        
        ServiceCollection serviceCollection = new();

        serviceCollection.AddSpawnJSRuntime(out SpawnJSRuntime runtime);
        serviceCollection.AddWebWorkerService(webWorkerService =>
        {
            webWorkerService.TaskPool.MaxPoolSize = -1;
            webWorkerService.TaskPool.PoolSize = webWorkerService.GlobalScope == GlobalScope.Window ? 2 : 0;
        });
            
        serviceCollection.AddSingleton<BrowserDirectoryManager>();
        serviceCollection.AddSingleton<BrowserFileManager>();
        
        serviceCollection.AddSingleton<IOPFSWorkerService, OPFSWorkerService>();
        
        ServiceProvider = serviceCollection.BuildServiceProvider();
        
        await ServiceProvider.StartBackgroundServices();
        
        UniScanApp.InitializePlatform += () =>
        {
            try
            {
                return Task.FromResult(new HostEnvironment(new BrowserPaths(), new BrowserPlatformSerilogInitializer(Constants.ConsoleOutputTemplate),
                                                           ServiceProvider.GetRequiredService<BrowserDirectoryManager>(), ServiceProvider.GetRequiredService<BrowserFileManager>()));
            }
            catch (Exception exception)
            {
                return Task.FromException<HostEnvironment>(exception);
            }
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