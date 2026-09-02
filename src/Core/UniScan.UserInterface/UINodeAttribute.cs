using Shiki.Common.Identity;

namespace UniScan.UserInterface;

[System.AttributeUsage(AttributeTargets.Class)]
public sealed class UINodeAttribute(string id) : Attribute
{
    public Identifier Identifier { get; } = Identifier.CreateInstance(id);
}