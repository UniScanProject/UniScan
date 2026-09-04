namespace UniScan.Client.Core.Remote.Connection.Status;

public readonly record struct UnexpectedDisconnectedConnectionStatusContext(
    Exception? Exception
) : IConnectionStatusContext
{
    public ConnectionState State => ConnectionState.UnexpectedDisconnected;
}