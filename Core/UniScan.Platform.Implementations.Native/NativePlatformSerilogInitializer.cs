using Serilog;

namespace UniScan.Platform.Implementations.Native;

public class NativePlatformSerilogInitializer(string consoleOutputTemplate, string fileOutputTemplate) : IPlatformSerilogInitializer
{
    public LoggerConfiguration GetConfiguration(HostEnvironment env) => new LoggerConfiguration()
                                                    .WriteTo.Console(outputTemplate: consoleOutputTemplate)
                                                    .WriteTo
                                                    .File(Path.Combine(env.StandardPaths.DataPath, "logs", "client", $"UniScan.Client-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log"),
                                                          retainedFileCountLimit: null,
                                                          outputTemplate: fileOutputTemplate)
    #if DEBUG
                                                    .MinimumLevel.Debug();
    #else
                                                    .MinimumLevel.Information();
    #endif
}