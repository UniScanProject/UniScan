namespace UniScan.Client.Core.Remote.Connection.Status;

public readonly record struct KickedDisconnectedConnectionStatusContext(
    string? Reason
) : IConnectionStatusContext
{
    public ConnectionState State => ConnectionState.KickedDisconnected;
}