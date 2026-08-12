using System.Text;
using MessagePack;
using Semver;
using Shiki.Common.Identity;
using Shiki.Extensions.MessagePack.Formatter.Identity;
using UniScan.Network.Formatter.SemVer;

namespace UniScan.Network.Data.Info.Software;

/// <summary>
/// Info about the client software
/// </summary>
/// <param name="Identifier">Client software identifier, used to differentiate client software projects</param>
/// <param name="Version">Client software version</param>
/// <param name="ProtocolVersion">Client protocol version, the server may reject unsupported protocol versions</param>
/// <param name="DisplayName">Display name of the client software</param>
/// <param name="Url">Download/Info URL of the client software</param>
[MessagePackObject]
public record ClientSoftwareInfo(
    [property: Key(0), MessagePackFormatter(typeof(IdentifierFormatter))] Identifier Identifier,
    [property: Key(1), MessagePackFormatter(typeof(SemVersionFormatter))] SemVersion Version,
    [property: Key(2)] int ProtocolVersion,
    [property: Key(3)] string DisplayName,
    [property: Key(4)] string? Url
) : ISoftwareInfo
{
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"{DisplayName} v{Version} ({Identifier}, PVN: {ProtocolVersion})");

        if (Url != null)
        {
            sb.Append($" {Url}");
        }
        
        return sb.ToString();
    }
}