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
}