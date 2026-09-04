namespace UniScan.Client.Core.Remote.Connection;

public enum ConnectionState
{
    /// <summary>
    /// Connecting to the server
    /// </summary>
    Connecting,
    /// <summary>
    /// Connected, but handshaking
    /// </summary>
    Handshaking,
    /// <summary>
    /// Connected, with handshake accepted
    /// </summary>
    Connected,
    /// <summary>
    /// Not yet connected
    /// </summary>
    NotConnected,
    /// <summary>
    /// Disconnected via user input
    /// </summary>
    UserDisconnected,
    /// <summary>
    /// Disconnected due to server kicking the client
    /// </summary>
    KickedDisconnected,
    /// <summary>
    /// Disconnected unexpectedly, happens when the socket dies unexpectedly or some exception is thrown
    /// </summary>
    UnexpectedDisconnected
}