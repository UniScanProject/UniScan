using System.Text.Json.Serialization;
using UniScan.Client.Core.Remote;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Data.Info.Remote;

namespace UniScan.Client.Core.Config.Remote;

[method: JsonConstructor]
public record CachedRemoteInfoDto(
    [property: JsonPropertyName("display_name")] string? DisplayName,
    [property: JsonPropertyName("logo_url")] Uri? LogoUrl)
{
    public static CachedRemoteInfoDto? FromRemoteServer(RemoteServer remoteServer) => remoteServer.RemoteInfo.Value != null ? new CachedRemoteInfoDto(remoteServer.RemoteInfo.Value.DisplayName, remoteServer.RemoteInfo.Value.Branding.LogoUrl) : null;

    //TODO replace
    public RemoteInfo ToSimpleRemoteInfo() => new(DisplayName, null, new RemoteBranding(LogoUrl, []), new RemoteSocial());
}

[method: JsonConstructor]
public record RemoteDto([property: JsonPropertyName("connectionMethod"), JsonRequired] IRemoteConnectionMethod ConnectionMethod)
{
    public static RemoteDto FromRemoteServer(RemoteServer remoteServer) => new(remoteServer.ConnectionMethod);
}