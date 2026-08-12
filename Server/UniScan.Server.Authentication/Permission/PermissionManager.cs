using System.Collections.Immutable;
using System.Data;
using Shiki.Common.Identity;

namespace UniScan.Server.Authentication.Permission;

public class PermissionManager
{
    /// <summary>
    /// Permissions that have been granted to the user
    /// </summary>
    private readonly Dictionary<Identifier, IPermission> _permissions = [];
    public ImmutableDictionary<Identifier, IPermission> Permissions => _permissions.ToImmutableDictionary();
    
    public void AddPermission(IPermission permission)
    {
        if (!_permissions.TryAdd(permission.Id, permission))
        {
            throw new ArgumentException($"Permission with id {permission.Id} is already registered");
        }
    }

    public void RemovePermission(IPermission permission) => _permissions.Remove(permission.Id);
    
    public IPermission? GetPermission(Identifier id)
    {
        if (!_permissions.TryGetValue(id, out IPermission? permission))
        {
            return null;
        }

        if (id != permission.Id)
        {
            throw new
                ConstraintException($"Found permission with requested ID key '{id}', however it's stored ID '{permission.Id}' does not match our key.");
        }

        return permission;
    }
}