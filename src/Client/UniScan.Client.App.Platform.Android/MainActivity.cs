using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia.Android;

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