using System;
using System.IO;
using Serilog;
using UniScan.Platform;

namespace UniScan.Client.App.Platform.Browser;

public class BrowserPlatformSerilogInitializer : IPlatformSerilogInitializer
{
    public LoggerConfiguration GetConfiguration(HostEnvironment env) => new LoggerConfiguration()
                                                    .WriteTo.Browser(outputTemplate: UniScan.Core.Constants.ConsoleOutputTemplate)
    #if DEBUG
                                                    .MinimumLevel.Debug();
    #else
                                                    .MinimumLevel.Information();
    #endif
}