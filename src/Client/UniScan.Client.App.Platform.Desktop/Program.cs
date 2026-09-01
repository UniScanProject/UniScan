using System;
using System.Threading.Tasks;
using Avalonia;
using UniScan.Platform;
using UniScan.Platform.Implementations.Native;
using UniScan.Platform.Implementations.Native.Filesystem;

namespace UniScan.Client.App.Platform.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        UniScanApp.InitializePlatform += () => Task.FromResult(new HostEnvironment(new DesktopPaths(), new NativePlatformSerilogInitializer(UniScan.Core.Constants.ConsoleOutputTemplate, UniScan.Core.Constants.FileOutputTemplate), new NativeDirectoryManager(), new NativeFileManager()));
        
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