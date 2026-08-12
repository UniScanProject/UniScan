using System.Buffers;
using Serilog;
using UniScan.Device.Connection.Protocol.Payload;

namespace UniScan.Device.Connection.Protocol;

public class PacketReceivedEventArgs(IScannerPayload payload) : EventArgs
{
    public IScannerPayload Payload { get; } = payload;
}

/// <summary>
/// Decodes received payloads from the remote scanner for the Connection to use.
/// </summary>
public interface IScannerProtocol
{
    /// <summary>
    /// Fired when a payload has been decoded into a Packet and is ready for handlers to use
    /// </summary>
    public event EventHandler<PacketReceivedEventArgs> PacketReceived;

    /// <summary>
    /// Logger for the protocol
    /// </summary>
    protected abstract ILogger Logger { get; set; }
    
    /// <summary>
    /// Decodes a payload and fires PacketReceived once usable
    /// </summary>
    /// <param name="buffer">The buffer from the remote stream</param>
    /// <returns>Whether or not the payload was handled successfully, a return value of false may indicate that extra data is required before handling the payload.</returns>
    bool HandlePayload(ref ReadOnlySequence<byte> buffer);

    /// <summary>
    /// Attempts to skip a payload, used for skipping invalid or unknown data
    /// </summary>
    /// <param name="buffer">The buffer from the remote stream</param>
    /// <returns>Whether skipping was successful</returns>
    bool TrySkipPayload(ref ReadOnlySequence<byte> buffer);
}