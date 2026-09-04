namespace UniScan.Client.Core.Remote.Connection;

public static class ConnectionStateExtensions
{
    extension(ConnectionState state)
    {
        public bool IsDisconnected() => state >= ConnectionState.NotConnected;
    }
}