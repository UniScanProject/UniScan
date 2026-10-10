using MessagePack;
using Shiki.Common.Factory;

namespace UniScan.Core.State.Node.Nodes;

public abstract class TypedStateNode<T>(T value) : IStateNode, IEquatable<TypedStateNode<T>>
{
    [Key(0)]
    public T Value { get; } = value;

    public override string ToString() => Value?.ToString() ?? "null";

    public bool Equals(TypedStateNode<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TypedStateNode<T>)obj);
    }

    public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Value);

    public static bool operator ==(TypedStateNode<T>? left, TypedStateNode<T>? right) => Equals(left, right);

    public static bool operator !=(TypedStateNode<T>? left, TypedStateNode<T>? right) => !Equals(left, right);
}

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/int8")]
[method: FactoryConstructable]
public partial class Int8StateNode(sbyte value) : TypedStateNode<sbyte>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/uint8")]
[method: FactoryConstructable]
public partial class UInt8StateNode(byte value) : TypedStateNode<byte>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/int16")]
[method: FactoryConstructable]
public partial class Int16StateNode(short value) : TypedStateNode<short>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/uint16")]
[method: FactoryConstructable]
public partial class UInt16StateNode(ushort value) : TypedStateNode<ushort>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/int32")]
[method: FactoryConstructable]
public partial class Int32StateNode(int value) : TypedStateNode<int>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/uint32")]
[method: FactoryConstructable]
public partial class UInt32StateNode(uint value) : TypedStateNode<uint>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/int64")]
[method: FactoryConstructable]
public partial class Int64StateNode(long value) : TypedStateNode<long>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/uint64")]
[method: FactoryConstructable]
public partial class UInt64StateNode(ulong value) : TypedStateNode<ulong>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/int128")]
[method: FactoryConstructable]
public partial class Int128StateNode(Int128 value) : TypedStateNode<Int128>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/uint128")]
[method: FactoryConstructable]
public partial class UInt128StateNode(UInt128 value) : TypedStateNode<UInt128>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/half")]
[method: FactoryConstructable]
public partial class HalfStateNode(Half value) : TypedStateNode<Half>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/float")]
[method: FactoryConstructable]
public partial class FloatStateNode(float value) : TypedStateNode<float>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/double")]
[method: FactoryConstructable]
public partial class DoubleStateNode(double value) : TypedStateNode<double>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/decimal")]
[method: FactoryConstructable]
public partial class DecimalStateNode(decimal value) : TypedStateNode<decimal>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/char")]
[method: FactoryConstructable]
public partial class CharStateNode(char value) : TypedStateNode<char>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/string")]
[method: FactoryConstructable]
public partial class StringStateNode(string value) : TypedStateNode<string>(value);

[MessagePackObject]
[StateNode("UniScan:device/state/node/typed/bool")]
[method: FactoryConstructable]
public partial class BooleanStateNode(bool value) : TypedStateNode<bool>(value);