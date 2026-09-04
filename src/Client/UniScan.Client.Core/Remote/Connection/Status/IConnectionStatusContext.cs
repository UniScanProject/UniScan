namespace UniScan.Client.Core.Remote.Connection.Status;

public interface IConnectionStatusContext
{
    ConnectionState State { get; }
}