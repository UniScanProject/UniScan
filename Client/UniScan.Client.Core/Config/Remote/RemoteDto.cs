using System.Text.Json.Serialization;
using UniScan.Client.Core.Config.Types;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Data.Info.Remote;

namespace UniScan.Client.Core.Config.Remote;

[method: JsonConstructor]
public record RemoteCacheDto(
    [property: JsonPropertyName("remote_info")]
    RemoteInfo? RemoteInfo);

[method: JsonConstructor]
public record RemoteDto([property: JsonPropertyName("connectionMethod"), JsonRequired] IRemoteConnectionMethod ConnectionMethod, [property: JsonPropertyName("cache"), JsonRequired] RemoteCacheDto Cache)
{
    public static RemoteDto FromRemoteServer(RemoteServer remoteServer) => new(remoteServer.ConnectionMethod, new RemoteCacheDto(remoteServer.RemoteInfo.CurrentValue));
}