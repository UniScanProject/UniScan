using System.Threading.Tasks;
using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using UniScan.Client.App.Platform.Android.Platform;
using UniScan.Core;
using UniScan.Platform;
using UniScan.Platform.Implementations.Native;
using UniScan.Platform.Implementations.Native.Filesystem;

namespace UniScan.Client.App.Platform.Android;

[Application]
public class Application : AvaloniaAndroidApplication<UniScanApp>
{
    protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        UniScanApp.InitializePlatform += () => Task.FromResult(new HostEnvironment(new AndroidPaths(), new AndroidPlatformSerilogInitializer(Constants.ConsoleOutputTemplate, Constants.FileOutputTemplate), new NativeDirectoryManager(), new NativeFileManager()));
        
        return base.CustomizeAppBuilder(builder)
                   .WithInterFont();
    }
}