using System.Reflection;
using Shiki.Common.Factory;
using Shiki.Common.Identity;
using UniScan.Core.State.Node;
using UniScan.Core.State.Node.Nodes;

namespace UniScan.Core.State;

public interface IStateSerializable
{
     void SerializeStatesInto(ref Dictionary<Identifier, IStateNode> states);//TODO codegen for this too

     Dictionary<Identifier, IStateNode> SerializeStates()
     {
         Dictionary<Identifier, IStateNode> states = [];
         SerializeStatesInto(ref states);

         return states;
     }
}

public static class StateSerializer
{
    public static IStateNode SerializeState(object state, Func<object, IStateNode> constructor)
    {
        return constructor(state);
    }
    
    public static IStateNode SerializeState<T>(T state, Func<T, IStateNode> constructor)
    {
        return constructor(state);
    }
    
    public static T SerializeState<T, TValue>(TValue state)
    where T : IFactoryConstructable<T, TValue>, IStateNode
    {
        return T.CreateInstance(state);
    }
    
    public static Dictionary<Identifier, IStateNode> SerializeContainer(object container)
    {
        if (container is IStateSerializable serializable)
        {
            return serializable.SerializeStates();
        }
        
        Dictionary<Identifier, IStateNode> nodes = new();
        
        PropertyInfo[] properties = container.GetType().GetProperties();
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

            IStateNode node = ctor.Invoke([prop.GetValue(container)]) as IStateNode ?? throw new InvalidOperationException();
            nodes[attr.Identifier] = node;
        }

        return nodes;
    }
}