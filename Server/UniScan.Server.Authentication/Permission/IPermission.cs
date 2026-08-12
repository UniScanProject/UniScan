using Shiki.Common.Identity;

namespace UniScan.Server.Authentication.Permission;

public interface IPermission
{
    public abstract Identifier Id { get; }
    public bool Allowed { get; }
}