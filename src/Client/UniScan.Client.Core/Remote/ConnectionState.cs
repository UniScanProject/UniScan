namespace UniScan.Client.Core.Remote;

public enum ConnectionState
{
    Connecting,
    Handshaking,
    Connected,
    Disconnected,
    UserDisconnected,
    KickedDisconnected,
    UnexpectedDisconnected
}

public interface IConnectionStatusContext
{
    ConnectionState State { get; }
}

public readonly record struct DefaultConnectionStatusContext(
    ConnectionState State
) : IConnectionStatusContext;

public readonly record struct KickedDisconnectedConnectionStatusContext(
    string? Reason
) : IConnectionStatusContext
{
    public ConnectionState State => ConnectionState.KickedDisconnected;
}

public readonly record struct UnexpectedDisconnectedConnectionStatusContext(
    Exception? Exception
) : IConnectionStatusContext
{
    public ConnectionState State => ConnectionState.UnexpectedDisconnected;
}