using System.Text.Json.Serialization;
using UniScan.Client.Core.Config.Types;
using UniScan.Network.Client.Remote.Connection;

namespace UniScan.Client.Core.Config.Remote;

[method: JsonConstructor]
public record RemoteDto([property: JsonPropertyName("connectionMethod")] [property: JsonRequired] IRemoteConnectionMethod ConnectionMethod)
{
    public static RemoteDto FromRemoteServer(RemoteServer remoteServer) => new(remoteServer.ConnectionMethod);
}