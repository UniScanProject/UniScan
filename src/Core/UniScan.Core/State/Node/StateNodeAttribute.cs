using Shiki.Common.Identity;

namespace UniScan.Core.State.Node;

[AttributeUsage(AttributeTargets.Class)]
public sealed class StateNodeAttribute : System.Attribute
{
    public Identifier Identifier { get; }
    
    public StateNodeAttribute(string str)
    {
        Identifier = Identifier.TryParseIntoResult(str).ExpectDefault();
    }
}