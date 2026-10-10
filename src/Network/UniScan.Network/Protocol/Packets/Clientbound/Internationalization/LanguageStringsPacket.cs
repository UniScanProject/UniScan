using MessagePack;
using Shiki.Common.Identity;
using UniScan.Network.Data.Info.Remote;
using UniScan.Network.Registry;

namespace UniScan.Network.Protocol.Packets.Clientbound.Internationalization;

/// <summary>
/// Sent to clients to append language strings to a given language id
/// </summary>
[RegistryPacket("UniScan", "packet", "clientbound", "internationalization", "language_strings")]
[MessagePackObject]
public readonly record struct LanguageStringsPacket(
    [property: Key(0)] string Id,
    [property: Key(1)] Dictionary<Identifier, string?> Strings
) : IClientboundPacket;