using System;
using System.IO;
using UniScan.Client.Core;
using UniScan.Platform;

namespace UniScan.Client.App.Platform.Android;

public class AndroidPaths : IPlatformStandardPaths
{
    public string DataPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public string ConfigPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
}