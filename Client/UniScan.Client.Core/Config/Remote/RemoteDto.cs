using System.Text.Json.Serialization;
using UniScan.Client.Core.Remote;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Data.Info.Remote;

namespace UniScan.Client.Core.Config.Remote;

[method: JsonConstructor]
public record RemoteCacheDto(
    [property: JsonPropertyName("remote_info")]
    RemoteInfo? RemoteInfo)
{
    public static RemoteCacheDto FromRemoteServer(RemoteServer remoteServer) => new(remoteServer.RemoteInfo.CurrentValue);
}

[method: JsonConstructor]
public record RemoteDto([property: JsonPropertyName("connectionMethod"), JsonRequired] IRemoteConnectionMethod ConnectionMethod)
{
    public static RemoteDto FromRemoteServer(RemoteServer remoteServer) => new(remoteServer.ConnectionMethod);
}