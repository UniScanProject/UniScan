using UniScan.Network.Protocol.Packets.Serverbound;

namespace UniScan.Network.Protocol.PayloadPart;

/// <summary>
/// The server will check that the client is properly connected and authenticated before processing packets that implement this interface.
///
/// If the client is not authenticated, then an AcknowledgePacket will be sent back with reason "Client is not authenticated."
/// </summary>
public interface IRequiresAuthenticationPayloadPart<TSelf> where TSelf : IServerboundPacket, IRequiresAuthenticationPayloadPart<TSelf>;