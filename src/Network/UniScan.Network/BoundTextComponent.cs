using MessagePack;
using Shiki.Common.Identity;
using Shiki.Common.Serialization.Polymorphism;
using Shiki.Internationalization;
using Shiki.Internationalization.Component;

namespace UniScan.Network;

[MessagePackObject]
public record BindableIdentifier([property: Key(0)] Identifier Identifier, [property: Key(1)] string? Fallback = null);

/// <summary>
/// Bindable string that does not resolve bindings
/// </summary>
/// <param name="identifier">Identifier of the binding to resolve</param>
[PolymorphicSerializable<BoundTextComponent, ITextComponent>]
[MessagePackObject]
public partial record BoundTextComponent([property: Key(0)] ITextComponent Format, [property: Key(1)] params BindableIdentifier[] Identifiers)
    : ITextComponent
{
    [SerializationConstructor]
    private BoundTextComponent() : this(new AbsoluteTextComponent("{0}"))
    {
    }
    
    private BoundTextComponent(params BindableIdentifier[] identifiers) : this(new AbsoluteTextComponent("{0}"), identifiers)
    {
    }
}

public static class IdentifierExtensions
{
    extension(Identifier id)
    {
        public BindableIdentifier AsBindableIdentifier(string? fallback = null) => new(id, fallback);
    }
}