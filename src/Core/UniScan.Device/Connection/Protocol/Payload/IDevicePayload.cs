namespace UniScan.Device.Connection.Protocol.Payload;

/// <summary>
/// Base class for a payload which can be sent/received from the remote Scanner
/// </summary>
public interface IDevicePayload
{
    /// <summary>
    /// Defines whether this payload is broadcasted, which will be ignored when dispatching to awaiting packet handler events
    /// </summary>
    public abstract bool IsBroadcast { get; }
}