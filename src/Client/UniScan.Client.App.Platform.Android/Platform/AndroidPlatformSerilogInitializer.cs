using System;
using System.IO;
using Serilog;
using UniScan.Platform;
using Constants = UniScan.Core.Constants;

namespace UniScan.Client.App.Platform.Android.Platform;

public class AndroidPlatformSerilogInitializer(string consoleOutputTemplate, string fileOutputTemplate) : IPlatformSerilogInitializer
{
    public LoggerConfiguration GetConfiguration(HostEnvironment env) => new LoggerConfiguration()
                                                                       .WriteTo.AndroidLog(outputTemplate: consoleOutputTemplate)
                                                                       .WriteTo
                                                                       .File(Path.Combine(env.StandardPaths.DataPath, "logs", "client", $"UniScan.Client-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log"),
                                                                             retainedFileCountLimit: 16,
                                                                             outputTemplate: fileOutputTemplate)
#if DEBUG
                                                                       .MinimumLevel.Debug();
#else
                                                    .MinimumLevel.Information();
#endif
}