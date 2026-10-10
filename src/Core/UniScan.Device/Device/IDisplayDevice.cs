using UniScan.Core.State.Display;

namespace UniScan.Device.Device;

public interface IDisplayDevice
{
    public IDisplay Display { get; }
}