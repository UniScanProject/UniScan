using System.Text.Json.Serialization;
using Shiki.Common.Util;
using UniScan.Device.Device;

namespace UniScan.Server.Core.Host;

[method: JsonConstructor]
public record ScannerHostDto(
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("scanner")] Scanner Scanner
)
{
    public ScannerHostDto(ScannerHost scannerHost) : this(scannerHost.DisplayName, scannerHost.Scanner) {}
}