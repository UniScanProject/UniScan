using Shiki.Common.Identity;

namespace UniScan.Network.Data.SSR;

[AttributeUsage(AttributeTargets.Class)]
public sealed class UISubscribeArgsAttribute(string id) : Attribute
{
    public Identifier Identifier { get; } = Identifier.CreateInstance(id);
}