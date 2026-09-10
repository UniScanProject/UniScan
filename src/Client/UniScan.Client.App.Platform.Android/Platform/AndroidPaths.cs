using System;
using System.IO;
using UniScan.Platform;

namespace UniScan.Client.App.Platform.Android.Platform;

public class AndroidPaths : IPlatformStandardPaths
{
    public string DataPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public string ConfigPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public string CachePath => Path.Combine(DataPath, ".cache");
}