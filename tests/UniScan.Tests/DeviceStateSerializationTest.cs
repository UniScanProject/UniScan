using Shiki.Common.Identity;
using UniScan.Core.State;
using UniScan.Core.State.Node;
using UniScan.Core.State.Node.Nodes;

namespace UniScan.Tests;

public class DeviceStateSerializationTest
{
    private record SomeDeviceState(    
        [property: StateProperty<Int32StateNode>("UniScan:tests/state/hello")]
        int Hello,
        
        [property: StateProperty<StringStateNode>("UniScan:tests/state/world")]
        string World
    ) : IDeviceState, IStateSerializable
    {
        public void SerializeStatesInto(ref Dictionary<Identifier, IStateNode> states)
        {
            states[Identifier.CreateInstance("UniScan:tests/state/hello")] = new Int32StateNode(Hello);
            states[Identifier.CreateInstance("UniScan:tests/state/world")] = new StringStateNode(World);
        }
    }

    [Test]
    public void Serialize()
    {
        SomeDeviceState st = new(100, "Hello, world!");
        var serialized = StateSerializer.SerializeContainer(st);
        
        foreach (var keyValuePair in serialized)
        {
            Console.WriteLine($"{keyValuePair.Key} -> {keyValuePair.Value}");

        }
    }
}