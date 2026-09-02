using Shiki.Common.Identity;

namespace UniScan.Network.Registry;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class RegistryPacketAttribute(Identifier id) : Attribute
{
    public Identifier Id { get; } = id;

    public RegistryPacketAttribute(string nmsp, string path) :  this(new Identifier(nmsp, path)) {}
    
    public RegistryPacketAttribute(string nmsp, params string[] path) : this(new Identifier(nmsp, path)) {}
}