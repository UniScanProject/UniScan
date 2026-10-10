using System.Reflection;
using MessagePack;
using MessagePack.Formatters;
using Shiki.Common.Identity;
using Shiki.Common.Serialization.Polymorphism;
using Shiki.Common.Serialization.Registry;
using Shiki.Common.Serialization.Registry.Validators;
using UniScan.UserInterface;

namespace UniScan.Network.Data.SSR;

public abstract class UISubscribeArgsTypeRegistryValidator : IHasAttributeTypeRegistryValidator<UISubscribeArgsAttribute>
{
    public static string GetKey(Type t) => t.GetCustomAttribute<UISubscribeArgsAttribute>()!.Identifier.ToString();
}


public class UISubscribeArgsTypeRegistry : SingletonTypeRegistry<UISubscribeArgsTypeRegistry, UISubscribeArgsTypeRegistryValidator, UISubscribeArgsAttribute>;//todo still kind of messy we should maybe make some generic IdentifiableAttributeTypeRegistry or something????

public class UISubscribeArgsMessagePackFormatter : IMessagePackFormatter<IUISubscribeArgs?>
{
    public void Serialize(ref MessagePackWriter writer, IUISubscribeArgs? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        Type t = value.GetType();
        UISubscribeArgsAttribute? attr = t.GetCustomAttribute<UISubscribeArgsAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException("Attempted to serialize subscribe args with no UISubscribeArgsAttribute");
        }
        
        MessagePackSerializer.Serialize(ref writer, attr.Identifier, options);
        MessagePackSerializer.Serialize(t, ref writer, value, options);
    }

    public IUISubscribeArgs? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
            return null;
        
        Identifier id = MessagePackSerializer.Deserialize<Identifier>(ref reader, options);
        
        Type? type = UISubscribeArgsTypeRegistry.Instance.GetType(id.ToString());
        if (type == null)
        {
            // return null;
            throw new MessagePackSerializationException("No type associated with this Identifier");
        }

        return (IUISubscribeArgs?)MessagePackSerializer.Deserialize(type, ref reader, options);
    }
}