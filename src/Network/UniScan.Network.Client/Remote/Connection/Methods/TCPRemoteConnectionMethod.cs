using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Shiki.Common.Serialization.Polymorphism;
using UniScan.Network.Client.Serialization;
using UniScan.Network.Group;

namespace UniScan.Network.Client.Remote.Connection.Methods;

#if !__WASM__
[PolymorphicSerializable<IRemoteConnectionMethod>("TCP")]
[method: JsonConstructor]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class TCPRemoteConnectionMethod(EndPoint endPoint) : IRemoteConnectionMethod
{
    [JsonConverter(typeof(EndPointConverter))]
    [JsonPropertyName("endpoint")]
    public EndPoint EndPoint { get; } = endPoint;

    public void Apply(Bootstrap bootstrap, INetworkGroupManager networkGroupManager)
    {
        bootstrap.Group(networkGroupManager.WorkerGroup)
                 .Channel<TcpSocketChannel>()
                 .Option(ChannelOption.TcpNodelay, true)
                 .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(5));
    }

    public void ApplyPipeline(IChannelPipeline pipeline)
    {
        pipeline.AddLast(new LengthFieldBasedFrameDecoder(DotNetty.Buffers.ByteOrder.LittleEndian, 1048576, 0, 4, 0, 4, true));
        pipeline.AddLast(new LengthFieldPrepender(DotNetty.Buffers.ByteOrder.LittleEndian, 4, 0, false));
    }

    public async Task<IChannel> ConnectAsync(Bootstrap bootstrap) => await bootstrap.ConnectAsync(EndPoint);

    public string ToDisplayString() => EndPoint.ToString();
}
#endif