using Semver;
using Shiki.Common.Identity;

namespace UniScan.Network.Data.Info.Software;

public interface ISoftwareInfo
{
    /// <summary>
    /// Server software identifier, should be unique per project
    /// </summary>
    Identifier Identifier { get; }
    /// <summary>
    /// Server software version
    /// </summary>
    SemVersion Version { get; }
    /// <summary>
    /// Server protocol version
    /// </summary>
    int ProtocolVersion { get; }
    /// <summary>
    /// Display name of the software
    /// </summary>
    string DisplayName { get; }
    /// <summary>
    /// Download/Info URL of the software
    /// </summary>
    string? Url { get; }
}