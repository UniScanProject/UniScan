using System.Runtime.CompilerServices;
using UniScan.Network.Data.Device;
using UniScan.Server.Core.Host;

namespace UniScan.Server.Core.Util;

public static class DeviceDtoExtensions
{
    extension(DeviceDto)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DeviceDto FromDeviceHost(DeviceHost host) =>
            DeviceDto.FromDevice(host.Identifier, host.Device, host.DisplayName);
    }
}