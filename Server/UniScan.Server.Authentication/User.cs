using Shiki.Common.Identity;
using UniScan.Server.Authentication.Permission;

namespace UniScan.Server.Authentication;

public class User
{
    public Identifier Id { get; }
    
    public Dictionary<Identifier, Group> Groups { get; } = new();
    public PermissionManager PermissionOverrides { get; } = new();
}
