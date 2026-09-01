using System.Reflection;
using Shiki.Common.Identity;
using UniScan.Core.State.Node;

namespace UniScan.Core.State;

public class DeviceStateSerializer
{
    public static Dictionary<Identifier, IDeviceStateNode> Serialize(DeviceState deviceState)
    {
        Dictionary<Identifier, IDeviceStateNode> nodes = new();
        
        PropertyInfo[] properties = typeof(DeviceState).GetProperties();
        foreach (PropertyInfo prop in properties)
        {
            StatePropertyAttribute? attr = prop.GetCustomAttribute<StatePropertyAttribute>();
            if (attr == null)
                continue;
            
            ConstructorInfo? ctor = attr.Type.GetConstructor([prop.PropertyType]);
            if (ctor == null)
            {
                throw new
                    InvalidOperationException($"Cannot serialize property '{prop.Name}' due to linked StateNode not containing accepting constructor");
            }

            IDeviceStateNode node = ctor.Invoke([prop.GetValue(deviceState)]) as IDeviceStateNode ?? throw new InvalidOperationException();
            nodes[attr.Identifier] = node;
        }

        return nodes;
    }
}