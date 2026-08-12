namespace UniScan.Device.Connection.Protocol.Payload.IO.Exception;

public enum PayloadParseError
{
    /// <summary>
    /// There is no error
    /// </summary>
    None,
    /// <summary>
    /// The payload data is incomplete, the handler should wait for more data and then attempt to parse again.
    /// </summary>
    Incomplete,
    /// <summary>
    /// The OpCode is invalid
    /// </summary>
    InvalidOpCode,
    /// <summary>
    /// The payload data is malformed/invalid
    /// </summary>
    Malformed
}