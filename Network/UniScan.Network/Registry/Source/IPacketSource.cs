namespace UniScan.Network.Registry.Source;

/// <summary>
/// Defines a source for list of typeof(IPacket) -> RegistryPacketAttribute
/// </summary>
public interface IPacketSource
{
    /// <summary>
    /// Gets packet types
    /// </summary>
    /// <returns>Packet types</returns>
    IReadOnlyDictionary<Type, RegistryPacketAttribute> GetPacketTypes();
}