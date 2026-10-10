using System.Reflection;
using MessagePack;
using MessagePack.Formatters;
using Shiki.Common.Identity;
using Shiki.Common.Serialization.Polymorphism;
using Shiki.Common.Serialization.Registry;
using Shiki.Common.Serialization.Registry.Validators;
using UniScan.UserInterface.Nodes.Core.Node;
using UniScan.UserInterface.Properties;

namespace UniScan.UserInterface.Formatting;

public abstract class UINodeTypeRegistryValidator : IHasAttributeTypeRegistryValidator<UINodeAttribute>
{
    public static string GetKey(Type t) => t.GetCustomAttribute<UINodeAttribute>()!.Identifier.ToString();
}


public class UINodeTypeRegistry : SingletonTypeRegistry<UINodeTypeRegistry, UINodeTypeRegistryValidator, UINodeAttribute>;

public class UINodeMessagePackFormatter : IMessagePackFormatter<IUINode?>
{
    public void Serialize(ref MessagePackWriter writer, IUINode? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        Type t = value.GetType();
        UINodeAttribute? attr = t.GetCustomAttribute<UINodeAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException("Attempted to serialize uinode with no UINodeAttribute");
        }
        
        MessagePackSerializer.Serialize(ref writer, attr.Identifier, options);
        MessagePackSerializer.Serialize(t, ref writer, value, options);
    }

    public IUINode? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
            return null;
        
        Identifier id = MessagePackSerializer.Deserialize<Identifier>(ref reader, options);
        
        Type? type = UINodeTypeRegistry.Instance.GetType(id.ToString());
        if (type == null)
        {
            throw new MessagePackSerializationException("No type associated with this Identifier");
        }

        return (IUINode?)MessagePackSerializer.Deserialize(type, ref reader, options);
    }
    
}