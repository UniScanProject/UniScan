using System.Buffers;
using System.Reflection;
using MessagePack;
using MessagePack.Formatters;
using Serilog;
using Shiki.Common.Identity;
using UniScan.Core.State.Node;
using UniScan.Core.State.Node.Nodes;

namespace UniScan.Network.Formatter.Device;

public class DeviceNodeFormatter : IMessagePackFormatter<IDeviceStateNode?>
{
    public void Serialize(ref MessagePackWriter writer, IDeviceStateNode? value, MessagePackSerializerOptions options)
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

    public IDeviceStateNode? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
            return null;
        
        Identifier id = MessagePackSerializer.Deserialize<Identifier>(ref reader, options);
        Type? type = GetType(id);

        if (type == null)
        {
            return new SerializedStateNode(id, reader.ReadBytes()?.ToArray() ?? []);
        }

        return (IDeviceStateNode?)MessagePackSerializer.Deserialize(type, ref reader, options);
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
                StateNodeAttribute? attr = type.GetCustomAttribute<StateNodeAttribute>();
                if (attr is StateNodeAttribute && attr.Identifier == id)
                {
                    return type;
                }
            }
        }

        return null;
    }
}