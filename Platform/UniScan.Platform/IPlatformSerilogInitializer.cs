using Serilog;

namespace UniScan.Platform;

public interface IPlatformSerilogInitializer
{
    LoggerConfiguration GetConfiguration(HostEnvironment env);
}