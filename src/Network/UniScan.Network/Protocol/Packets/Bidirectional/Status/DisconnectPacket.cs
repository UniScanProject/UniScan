using MessagePack;
using UniScan.Network.Registry;

namespace UniScan.Network.Protocol.Packets.Bidirectional.Status;

/// <summary>
/// Sent by the client to signal that it is disconnecting from the server for a given reason
///
/// Sent by the server to disconnect a client for a given reason
/// </summary>
/// <param name="Reason">The reason</param>
[RegistryPacket("UniScan", "packet", "bidirectional", "status", "disconnect")]
[MessagePackObject]
public readonly record struct DisconnectPacket(
    [property: Key(0)] string Reason
) : IBidirectionalPacket;