using System.Reflection;
using MessagePack;
using MessagePack.Formatters;
using Shiki.Common.Identity;

namespace UniScan.UserInterface.Formatting;

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
        
        Type? type = GetType(id);
        if (type == null)
        {
            throw new MessagePackSerializationException("No type associated with this Identifier");
        }

        return (IUINode?)MessagePackSerializer.Deserialize(type, ref reader, options);
    }

    //TODO TEMPORARY, I HOPE.
    //reflection is very slow especially when we're fucking looping all loaded execs EVERY TIME WE NEED TO DESERIALIZE
    //instead I will just make some registry i guess
    public Type? GetType(Identifier id)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                UINodeAttribute? attr = type.GetCustomAttribute<UINodeAttribute>();
                if (attr != null && attr.Identifier == id)
                {
                    return type;
                }
            }
        }

        return null;
    }
}