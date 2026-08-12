using Shiki.Common.Identity;
using Shiki.Common.Util;

namespace UniScan.Network;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class RegistryPacketAttribute(Identifier id) : Attribute
{
    public Identifier Id { get; } = id;

    public RegistryPacketAttribute(string nmsp, string path) :  this(new Identifier(nmsp, path)) {}
    
    public RegistryPacketAttribute(string nmsp, params string[] path) : this(new Identifier(nmsp, path)) {}
}