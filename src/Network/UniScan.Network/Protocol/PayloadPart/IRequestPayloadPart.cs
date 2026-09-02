namespace UniScan.Network.Protocol.PayloadPart;

/// <summary>
/// A payload part containing a Request ID
/// </summary>
public interface IRequestIdPayloadPart
{
    /// <summary>
    /// Used as a way to know what the server is responding to
    /// </summary>
    /// 
    /// <p>
    /// When a packet is received by the server with a RequestId, assuming the packet supports it, the server will echo back the same RequestId in its response.
    /// 
    /// The client is to attach a request ID and a special handler on send, when the client receives a packet with the same request id, the handler will handle it.
    /// </p>
    Guid? RequestId { get; }
}

/// <summary>
/// Adds a RequestId that the packet will hold when responding to a request
/// </summary>
public interface IResponsePayloadPart : IRequestIdPayloadPart;

/// <summary>
/// Adds a RequestId that the server will echo back in the response packet(s)
///
/// <typeparam name="TResponse">The expected response type from the server</typeparam>
/// </summary>
public interface IRequestPayloadPart<out TResponse> : IRequestIdPayloadPart
    where TResponse : IPacket, IResponsePayloadPart;