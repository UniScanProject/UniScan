using System.Text.Json;
using Serilog;
using Shiki.Common.Serialization.Polymorphism;
using Shiki.Common.Serialization.Polymorphism.Source.Sources;

namespace UniScan.Core.Serialization;

public static class PolymorphicJsonOptionsFactory
{
    public static JsonSerializerOptions Get()
    {
        //load types for scanner polymorphism
        var types = AssembliesPolymorphicTypeSource.Load();
        foreach (var type in types)
        {
            Log.Debug("Registering polymorphic type '{Id}' ({Type}) for base '{BaseType}'", type.Value.Id, type.Key.Name, type.Value.BaseType);
        }

        return new JsonSerializerOptions
        {
            TypeInfoResolver = new PolymorphicTypeInfoResolver(types)
        };
    }
}