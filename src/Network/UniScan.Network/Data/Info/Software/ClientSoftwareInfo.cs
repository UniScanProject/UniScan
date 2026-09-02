using System.Text;
using MessagePack;
using Semver;
using Shiki.Common.Identity;
using Shiki.Extensions.MessagePack.Formatter.Identity;
using UniScan.Network.Formatter.Semver;

namespace UniScan.Network.Data.Info.Software;

[MessagePackObject]
public record SoftwareAssemblyInfo(
    [property: Key(0), MessagePackFormatter(typeof(IdentifierMessagePackFormatter))]
    Identifier Identifier,
    [property: Key(1), MessagePackFormatter(typeof(SemVersionFormatter))]
    SemVersion Version
);

/// <summary>
/// Info about the client software
/// </summary>
/// <param name="AppInfo">Client assembly info, used to differentiate client software projects</param>
/// <param name="PlatformInfo">Client platform info, version field should be 0.0.0 if not desired.</param>
/// <param name="ProtocolVersion">Client protocol version, the server may reject unsupported protocol versions</param>
/// <param name="DisplayName">Display name of the client software</param>
/// <param name="Url">Download/Info URL of the client software</param>
[MessagePackObject]
public record ClientSoftwareInfo(
    [property: Key(0)] SoftwareAssemblyInfo AppInfo,
    [property: Key(1)] SoftwareAssemblyInfo PlatformInfo,
    [property: Key(2)] int ProtocolVersion,
    [property: Key(3)] string DisplayName,
    [property: Key(4)] string? Url
) : IClientSoftwareInfo
{
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"{DisplayName} v{AppInfo.Version} ({AppInfo.Identifier} // {PlatformInfo.Identifier} v{PlatformInfo.Version}, PVN: {ProtocolVersion})");

        if (Url != null)
        {
            sb.Append($" {Url}");
        }
        
        return sb.ToString();
    }
}