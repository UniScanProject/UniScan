using System.Net;
using System.Text.Json.Serialization;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Shiki.Common.Serialization.Polymorphism;
using UniScan.Network.Client.Serialization;

namespace UniScan.Network.Client.Remote.Connection.Methods;

#if !__WASM__
[PolymorphicSerializable<IRemoteConnectionMethod>("TCP")]
public class TCPRemoteConnectionMethod(IPEndPoint endPoint) : IRemoteConnectionMethod
{
    [JsonConverter(typeof(IPEndPointConverter))]
    [JsonPropertyName("endpoint")]
    public IPEndPoint EndPoint { get; } = endPoint;

    public void Apply(Bootstrap bootstrap)
    {
        bootstrap.Channel<TcpSocketChannel>()
                 .Option(ChannelOption.TcpNodelay, true)
                 .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(5));
    }

    public Task<IChannel> ConnectAsync(Bootstrap bootstrap) => bootstrap.ConnectAsync(EndPoint);

    public string ToDisplayString() => EndPoint.ToString();
}
#endif