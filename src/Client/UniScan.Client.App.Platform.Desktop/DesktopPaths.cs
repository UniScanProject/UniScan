using System;
using System.IO;
using UniScan.Platform;

namespace UniScan.Client.App.Platform.Desktop;

public class DesktopPaths : IPlatformStandardPaths
{
    public string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                           "UniScan");

    public string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                             "UniScan");
    
    public string CachePath => OperatingSystem.IsLinux() ?
                                   Path.Combine(Environment.GetEnvironmentVariable("XDG_CACHE_HOME") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache"), "UniScan")
                                   : Path.Combine(DataPath, "cache");
}