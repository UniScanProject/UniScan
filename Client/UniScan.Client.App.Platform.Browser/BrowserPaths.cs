using System;
using System.IO;
using UniScan.Client.Core;
using UniScan.Platform;

namespace UniScan.Client.App.Platform.Browser;

public class BrowserPaths : IPlatformStandardPaths
{
    public string DataPath => "data";
    public string ConfigPath => "config";
}