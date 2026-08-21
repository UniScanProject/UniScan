namespace UniScan.Client.App.ViewModels;

public class LoadingViewModel : ViewModelBase
{
    public static string VersionString => $"UniScan Client v{UniScanApp.SoftwareInfo.Version} (Platform v{UniScanApp.PlatformVersion})";
    
    public UniScanApp App { get; }

    public LoadingViewModel(UniScanApp app)
    {
        App = app;
    }
}