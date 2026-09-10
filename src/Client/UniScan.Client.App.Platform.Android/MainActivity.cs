using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using UniScan.Platform;
using UniScan.Platform.Implementations.Native;
using UniScan.Platform.Implementations.Native.Filesystem;


namespace UniScan.Client.App.Platform.Android;

[Activity(
             Label = "UniScan",
             Theme = "@style/MyTheme.NoActionBar",
             Icon = "@drawable/icon",
             MainLauncher = true,
             ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
[IntentFilter(["android.intent.action.MAIN"], Categories = ["android.intent.category.LEANBACK_LAUNCHER"])]
public class MainActivity : AvaloniaMainActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
    }
}