using System.Buffers;
using System.Reflection;
using MessagePack;
using MessagePack.Formatters;
using Serilog;
using Shiki.Common.Identity;
using Shiki.Common.Serialization.Polymorphism;
using Shiki.Common.Serialization.Registry;
using Shiki.Common.Serialization.Registry.Validators;
using UniScan.Core.State.Node;
using UniScan.Core.State.Node.Nodes;

namespace UniScan.Network.Formatter.Device;

public abstract class DeviceNodeTypeRegistryValidator : IHasAttributeTypeRegistryValidator<StateNodeAttribute>
{
    static string ITypeRegistryValidator.GetKey(Type t) => t.GetCustomAttribute<StateNodeAttribute>()!.Identifier.ToString();
}


public class DeviceNodeTypeRegistry : SingletonTypeRegistry<DeviceNodeTypeRegistry, DeviceNodeTypeRegistryValidator, StateNodeAttribute>;

public class DeviceNodeFormatter : IMessagePackFormatter<IStateNode?>
{
    public void Serialize(ref MessagePackWriter writer, IStateNode? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        Type t = value.GetType();
        StateNodeAttribute? attr = t.GetCustomAttribute<StateNodeAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException("Attempted to serialize node with no StateNodeAttribute");
        }
        
        MessagePackSerializer.Serialize(ref writer, attr.Identifier, options);
        MessagePackSerializer.Serialize(t, ref writer, value, options);
    }

    public IStateNode? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
            return null;
        
        Identifier id = MessagePackSerializer.Deserialize<Identifier>(ref reader, options);
        Type? type = DeviceNodeTypeRegistry.Instance.GetType(id.ToString());

        if (type == null)
        {
            return new SerializedStateNode(id, reader.ReadBytes()?.ToArray() ?? []);
        }

        return (IStateNode?)MessagePackSerializer.Deserialize(type, ref reader, options);
    }
}