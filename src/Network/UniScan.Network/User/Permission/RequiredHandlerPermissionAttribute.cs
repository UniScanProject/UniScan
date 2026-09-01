using Shiki.Common.Identity;
using Shiki.Common.Util;

namespace UniScan.Network.User.Permission;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class RequiredHandlerPermissionAttribute(Identifier id) : Attribute
{
    public Identifier Id { get; } = id;

    public RequiredHandlerPermissionAttribute(string nmsp, string path) :  this(new Identifier(nmsp, path)) {}
    
    public RequiredHandlerPermissionAttribute(string nmsp, params string[] path) : this(new Identifier(nmsp, path)) {}
}