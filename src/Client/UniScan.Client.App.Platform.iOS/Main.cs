using System.Threading.Tasks;
using UIKit;
using UniScan.Platform;
using UniScan.Platform.Implementations.Native;
using UniScan.Platform.Implementations.Native.Filesystem;

namespace UniScan.Client.App.Platform.iOS;

public class Application
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        UniScanApp.InitializePlatform += () => Task.FromResult(new HostEnvironment(new iOSPaths(), new NativePlatformSerilogInitializer(UniScan.Core.Constants.ConsoleOutputTemplate, UniScan.Core.Constants.FileOutputTemplate), new NativeDirectoryManager(), new NativeFileManager()));

        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}