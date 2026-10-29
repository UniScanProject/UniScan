#if UNISCAN_SERVER
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Shiki.Common.Serialization.Polymorphism;
using UniScan.Network.Group;
using UniScan.Network.Server.HostMethod;
using UniScan.Network.Util;

namespace UniScan.Modules.WebsocketTransportMethod.Network;

public record TlsCertificateDto(
    [property: JsonPropertyName("cert_path"), JsonRequired] string CertificatePath,
    [property: JsonPropertyName("key_path")] string? PrivateKeyPath
);

[PolymorphicSerializable<IServerHostMethod>("Websocket")]
[method: JsonConstructor]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class WebsocketServerHostMethod(IPAddress? @interface, ushort port = UniScan.Network.Constants.DefaultPort) : IServerHostMethod
{
    [JsonPropertyName("interface")]
    [JsonConverter(typeof(IPAddressJsonConverter))]
    public IPAddress Interface { get; } = @interface ?? IPAddress.Any;
    
    [JsonPropertyName("port")]
    public ushort Port { get; } = port;

    [JsonPropertyName("path")]
    public string EndpointPath { get; init; } = "/";
    
    [JsonPropertyName("ssl_cert")]
    public TlsCertificateDto? Certificate { get; init; }
    
    public WebsocketServerHostMethod(ushort port = UniScan.Network.Constants.DefaultPort) : this(IPAddress.Any, port) {}

    public void Apply(ServerBootstrap bootstrap, IHostNetworkGroupManager groupManager)
    {
        bootstrap.Channel<TcpServerSocketChannel>()
                 .Group(groupManager.MasterGroup, groupManager.WorkerGroup)
                 .Option(ChannelOption.SoBacklog, 100)
                 .ChildOption(ChannelOption.TcpNodelay, true)
                 .ChildOption(ChannelOption.SoKeepalive, true);
    }

    public void ApplyPipeline(IChannelPipeline pipeline)
    {
        // framing
        // pipeline.AddLast(new LengthFieldBasedFrameDecoder(DotNetty.Buffers.ByteOrder.LittleEndian, 1048576, 0, 4, 0, 4, true));
        // pipeline.AddLast(new LengthFieldPrepender(DotNetty.Buffers.ByteOrder.LittleEndian, 4, 0, false));
        
        if (Certificate is not null)
        {
            if (File.Exists(Certificate.CertificatePath) && File.Exists(Certificate.PrivateKeyPath))
            {
                pipeline.AddLast(TlsHandler.Server(X509Certificate2.CreateFromPemFile(Path.GetFullPath(Certificate.CertificatePath), Path.GetFullPath(Certificate.PrivateKeyPath))));
            }
        }

        pipeline.AddLast(new HttpServerCodec());
        pipeline.AddLast(new HttpObjectAggregator(65536));
        pipeline.AddLast(new WebSocketServerProtocolHandler(EndpointPath, null, true));
        pipeline.AddLast(new WebsocketFrameAdapter());
    }

    public async Task<IChannel> BindAsync(ServerBootstrap bootstrap) => await bootstrap.BindAsync(Interface, Port);

    public string ToDisplayString() => $"{(Certificate != null ? "wss" : "ws")}://{Interface}:{Port}";
}
#endif