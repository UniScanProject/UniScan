using System.Text;
using MessagePack;
using Semver;
using Shiki.Common.Identity;
using Shiki.Extensions.MessagePack.Formatter.Identity;
using UniScan.Network.Formatter.SemVer;

namespace UniScan.Network.Data.Info.Software;

/// <summary>
/// Info about the server software
/// </summary>
/// <param name="AppInfo">Server software info, used to differentiate server software projects</param>
/// <param name="ProtocolVersion">Server protocol version, may vary as clients are not supposed to take action based on this field.</param>
/// <param name="DisplayName">Display name of the server software</param>
/// <param name="Url">Download/Info URL of the server software</param>
[MessagePackObject]
public record ServerSoftwareInfo(
    [property: Key(0)] SoftwareAssemblyInfo AppInfo,
    [property: Key(1)] int ProtocolVersion,
    [property: Key(2)] string DisplayName,
    [property: Key(3)] string? Url
) : ISoftwareInfo
{
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"{DisplayName} v{AppInfo.Version} ({AppInfo.Identifier}, PVN: {ProtocolVersion})");

        if (Url != null)
        {
            sb.Append($" {Url}");
        }
        
        return sb.ToString();
    }
}