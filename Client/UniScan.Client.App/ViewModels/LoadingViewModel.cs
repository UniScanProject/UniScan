using System.Reflection;
using Semver;
using Shiki.Common.Extensions;

namespace UniScan.Client.App.ViewModels;

public class LoadingViewModel : ViewModelBase
{
    public static string VersionString => $"UniScan Client v{UniScanApp.SoftwareInfo.Version} (Platform v{UniScanApp.PlatformVersion})";
}