#if UNISCAN_CLIENT
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Shiki.Common.Serialization.Polymorphism;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Group;

namespace UniScan.Modules.WebSocketTransportMethod.Network;

[PolymorphicSerializable<IRemoteConnectionMethod>("WebSocket")]
[method: JsonConstructor]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class WebSocketRemoteConnectionMethod(Uri uri) : IRemoteConnectionMethod
{
    [JsonPropertyName("url")]
    public Uri Uri { get; } = uri;

    public void Apply(Bootstrap bootstrap, INetworkGroupManager networkGroupManager)
    {
        bootstrap.Group(networkGroupManager.WorkerGroup)
                 .Channel<TcpSocketChannel>()
                 .Option(ChannelOption.TcpNodelay, true)
                 .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(5));
    }

    public void ApplyPipeline(IChannelPipeline pipeline)
    {
        if (Uri.Scheme == Uri.UriSchemeHttps || Uri.Scheme == Uri.UriSchemeWss)
        {
            pipeline.AddLast(new TlsHandler(new ClientTlsSettings(Uri.Host)));
        }
        
        WebSocketClientHandshaker h = WebSocketClientHandshakerFactory.NewHandshaker(
                                                               Uri,
                                                               WebSocketVersion.V13,
                                                               null,
                                                               false,
                                                               new DefaultHttpHeaders()
                                                              );

        pipeline.AddLast(new HttpClientCodec());
        pipeline.AddLast(new HttpObjectAggregator(8192));
        pipeline.AddLast(new WebSocketClientProtocolHandler(h));
        pipeline.AddLast(new WebSocketHandshakeAwaiter());
        pipeline.AddLast(new WebSocketFrameAdapter());
    }

    public async Task<IChannel> ConnectAsync(Bootstrap bootstrap)
    {
        EndPoint e = IPAddress.TryParse(Uri.Host, out IPAddress? addr)
                         ? new IPEndPoint(addr, Uri.Port)
                         : new DnsEndPoint(Uri.Host, Uri.Port);

        IChannel c = await bootstrap.ConnectAsync(e);

        WebSocketHandshakeAwaiter? w = c.Pipeline.Get<WebSocketHandshakeAwaiter>();
        if (w != null)
            await w.Task;//wait for handshake otherwise we will throw some stupid exception

        return c;
    }

    public string ToDisplayString() => Uri.ToString();
}
#endif