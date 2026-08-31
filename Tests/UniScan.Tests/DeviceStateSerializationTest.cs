using UniScan.Core.State;

namespace UniScan.Tests;

public class DeviceStateSerializationTest
{
    [Test]
    public void Serialize()
    {
        DeviceState st = new DeviceState();
        var serialized = DeviceStateSerializer.Serialize(st);
        
        Console.WriteLine(serialized);
    }
}