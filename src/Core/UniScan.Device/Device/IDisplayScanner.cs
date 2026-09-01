using UniScan.Core.State.Display;

namespace UniScan.Device.Device;

public interface IDisplayScanner
{
    public IDisplay Display { get; }
}