using MessagePack;

namespace UniScan.Core.State.Node.Nodes;

public abstract class TypedStateNode<T>(T value) : IDeviceStateNode
{
    [Key(0)]
    public T Value { get; } = value;

    public override string ToString() => Value?.ToString() ?? "null";
}

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/int8")]
public class Int8StateNode(sbyte value) : TypedStateNode<sbyte>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/uint8")]
public class Uint8StateNode(byte value) : TypedStateNode<byte>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/int16")]
public class Int16StateNode(short value) : TypedStateNode<short>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/uint16")]
public class Uint16StateNode(ushort value) : TypedStateNode<ushort>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/int32")]
public class Int32StateNode(int value) : TypedStateNode<int>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/uint32")]
public class Uint32StateNode(uint value) : TypedStateNode<uint>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/int64")]
public class Int64StateNode(long value) : TypedStateNode<long>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/uint64")]
public class Uint64StateNode(ulong value) : TypedStateNode<ulong>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/string")]
public class StringStateNode(string value) : TypedStateNode<string>(value);