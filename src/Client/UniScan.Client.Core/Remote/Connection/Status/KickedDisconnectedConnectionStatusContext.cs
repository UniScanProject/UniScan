using Shiki.Internationalization.Component;

namespace UniScan.Client.Core.Remote.Connection.Status;

public readonly record struct KickedDisconnectedConnectionStatusContext(
    ITextComponent? Reason
) : IConnectionStatusContext
{
    public ConnectionState State => ConnectionState.KickedDisconnected;
}