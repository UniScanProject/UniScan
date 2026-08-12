using Shiki.Common.Identity;

namespace UniScan.Core;

public static class Constants
{
    /// <summary>
    /// Serilog console output line template
    /// </summary>
    public const string ConsoleOutputTemplate =
        "{Timestamp:HH:mm:ss} [{Level:u3} | {SourceContext}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Serilog file output line template
    /// </summary>
    public const string FileOutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3} | {SourceContext}] {Message:lj}{NewLine}{Exception}";

    public static readonly IdentifierNamespace IdentifierNamespace = new("UniScan");
}