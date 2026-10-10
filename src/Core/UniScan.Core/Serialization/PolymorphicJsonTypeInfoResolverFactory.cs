using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Shiki.Common.Serialization.Polymorphism;

namespace UniScan.Core.Serialization;

public static class PolymorphicJsonTypeInfoResolverFactory
{
    public static IJsonTypeInfoResolver Get(IJsonTypeInfoResolver resolver)
        => resolver.WithAddedModifier(typeInfo =>
        {
            if (!PolymorphicTypeRegistry.GlobalTypes.TryGetValue(typeInfo.Type, out var derived)) return;
            JsonPolymorphismOptions opt = new()
            {
                TypeDiscriminatorPropertyName = "$type",
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
            };

            foreach (var kvp in derived)
            {
                opt.DerivedTypes.Add(new JsonDerivedType(kvp.Value, kvp.Key));
            }

            typeInfo.PolymorphismOptions = opt;
        });
}