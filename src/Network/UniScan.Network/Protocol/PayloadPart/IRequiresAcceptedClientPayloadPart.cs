using UniScan.Network.Protocol.Packets.Serverbound;

namespace UniScan.Network.Protocol.PayloadPart;

/// <summary>
/// DO NOT INHERIT FROM THIS, INSTEAD INHERIT FROM IRequiresAcceptedClientPayloadPart&lt;TSelf&gt;
/// The server will check that the client is properly connected and on a supported version before processing packets that implement this interface.
///
/// If the client is not in the connected list, then an AcknowledgePacket will be sent back with reason "Client has not completed handshake."
/// </summary>
public interface IRequiresAcceptedClientPayloadPart;

/// <summary>
/// The server will check that the client is properly connected and on a supported version before processing packets that implement this interface.
///
/// If the client is not in the connected list, then an AcknowledgePacket will be sent back with reason "Client has not completed handshake."
/// </summary>
public interface IRequiresAcceptedClientPayloadPart<TSelf> : IRequiresAcceptedClientPayloadPart
    where TSelf : IServerboundPacket, IRequiresAcceptedClientPayloadPart<TSelf>;