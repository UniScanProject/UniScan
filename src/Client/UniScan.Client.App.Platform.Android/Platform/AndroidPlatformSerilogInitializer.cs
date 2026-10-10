using System;
using System.IO;
using Serilog;
using UniScan.Platform;

namespace UniScan.Client.App.Platform.Android.Platform;

public class AndroidPlatformSerilogInitializer(string consoleOutputTemplate, string fileOutputTemplate, string dataPath) : IPlatformSerilogInitializer
{
    public LoggerConfiguration GetConfiguration() => new LoggerConfiguration()
                                                                       .WriteTo.AndroidLog(outputTemplate: consoleOutputTemplate)
                                                                       .WriteTo
                                                                       .File(Path.Combine(dataPath, "logs", "client", $"UniScan.Client-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log"),
                                                                             retainedFileCountLimit: 16,
                                                                             outputTemplate: fileOutputTemplate)
#if DEBUG
                                                                       .MinimumLevel.Debug();
#else
                                                    .MinimumLevel.Information();
#endif
}