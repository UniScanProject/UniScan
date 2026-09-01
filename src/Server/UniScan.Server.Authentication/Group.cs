using Shiki.Common.Identity;
using UniScan.Server.Authentication.Permission;

namespace UniScan.Server.Authentication;

public class Group
{
    public Identifier Id { get; }
    
    public PermissionManager PermissionManager { get; } = new();
}