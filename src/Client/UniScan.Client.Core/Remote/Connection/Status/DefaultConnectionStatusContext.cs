namespace UniScan.Client.Core.Remote.Connection.Status;

public readonly record struct DefaultConnectionStatusContext(
    ConnectionState State
) : IConnectionStatusContext;