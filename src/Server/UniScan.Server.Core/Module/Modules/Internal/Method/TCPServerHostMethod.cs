using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Shiki.Common.Serialization.Polymorphism;
using UniScan.Network.Group;
using UniScan.Network.Server.HostMethod;
using UniScan.Network.Util;

namespace UniScan.Server.Core.Module.Modules.Internal.Method;

#if !__WASM__
[PolymorphicSerializable<IServerHostMethod>("TCP")]
[method: JsonConstructor]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public class TCPServerHostMethod(IPAddress? @interface, ushort port = Network.Constants.DefaultPort) : IServerHostMethod
{
    [JsonPropertyName("interface")]
    [JsonConverter(typeof(IPAddressJsonConverter))]
    public IPAddress Interface { get; } = @interface ?? IPAddress.Any;
    
    [JsonPropertyName("port")]
    public ushort Port { get; } = port;
    
    public TCPServerHostMethod(ushort port = Network.Constants.DefaultPort) : this(IPAddress.Any, port) {}

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
        pipeline.AddLast(new LengthFieldBasedFrameDecoder(DotNetty.Buffers.ByteOrder.LittleEndian, 1048576, 0, 4, 0, 4, true));
        pipeline.AddLast(new LengthFieldPrepender(DotNetty.Buffers.ByteOrder.LittleEndian, 4, 0, false));
    }

    public async Task<IChannel> BindAsync(ServerBootstrap bootstrap) => await bootstrap.BindAsync(Interface, Port);

    public string ToDisplayString() => $"TCP {Interface}:{Port}";
}
#endif