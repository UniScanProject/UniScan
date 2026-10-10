using UniScan.Core.State;
using UniScan.Core.State.Radio;
using UniScan.Server.Core.Host;

namespace UniScan.Server.Core.UserInterface;

public class DeviceLayoutContext(DeviceHost host, IDeviceState? state) : ILayoutContext
{
    public DeviceHost Host { get; } = host;
    public IDeviceState? State { get; } = state;
}

public class ScanChannelLayoutContext(DeviceHost host, IDeviceState? state) : ILayoutContext
{
    public DeviceHost Host { get; } = host;
    public IDeviceState? State { get; } = state;
}

public class ScanGroupLayoutContext(DeviceHost host, IDeviceState? state) : ILayoutContext
{
    public DeviceHost Host { get; } = host;
    public IDeviceState? State { get; } = state;
}

public class ScanListLayoutContext(DeviceHost host, IDeviceState? state) : ILayoutContext
{
    public DeviceHost Host { get; } = host;
    public IDeviceState? State { get; } = state;
}

public class ScanZoneLayoutContext(DeviceHost host, IDeviceState? state) : ILayoutContext
{
    public DeviceHost Host { get; } = host;
    public IDeviceState? State { get; } = state;
}