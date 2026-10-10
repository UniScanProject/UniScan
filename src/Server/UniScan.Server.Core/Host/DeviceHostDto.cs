using System.Text.Json.Serialization;
using Shiki.Common.Util;
using UniScan.Device.Device;

namespace UniScan.Server.Core.Host;

[method: JsonConstructor]
public record DeviceHostDto(
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("device")] Device.Device.Device Device
)
{
    public DeviceHostDto(DeviceHost deviceHost) : this(deviceHost.DisplayName, deviceHost.Device) {}
}