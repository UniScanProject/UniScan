namespace UniScan.Network.Data.Info.Software;

public interface IClientSoftwareInfo : ISoftwareInfo
{
    /// <summary>
    /// Info on the client's platform
    /// </summary>
    SoftwareAssemblyInfo PlatformInfo { get; }
}