using System.Runtime.Versioning;
using Serilog;

namespace UniScan.Platform.Implementations.Web;

[SupportedOSPlatform("browser")]
public class BrowserPlatformSerilogInitializer(string consoleOutputTemplate) : IPlatformSerilogInitializer
{
    public LoggerConfiguration GetConfiguration(HostEnvironment env) => new LoggerConfiguration()
                                                    .WriteTo.Browser(outputTemplate: consoleOutputTemplate)
    #if DEBUG
                                                    .MinimumLevel.Debug();
    #else
                                                    .MinimumLevel.Information();
    #endif
}