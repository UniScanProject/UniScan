using Shiki.Common.Identity;

namespace UniScan.Core.State.Node;

public abstract class StatePropertyAttribute(string id, Type type) : System.Attribute
{
    public Identifier Identifier { get; } = Identifier.TryParseIntoResult(id).ExpectDefault();
    public Type Type { get; } = type;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class StatePropertyAttribute<T>(string id) : StatePropertyAttribute(id, typeof(T))
where T : IDeviceStateNode;