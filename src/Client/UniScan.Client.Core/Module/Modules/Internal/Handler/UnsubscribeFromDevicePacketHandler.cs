using DotNetty.Transport.Channels;
using ObservableCollections;
using Serilog;
using Shiki.Common.Identity;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Device;
using UniScan.Core.State;
using UniScan.Core.State.Node;
using UniScan.Network.Protocol.Packets.Clientbound.Device;

namespace UniScan.Client.Core.Module.Modules.Internal.Handler;

public class UnsubscribeFromDevicePacketHandler : SimpleChannelInboundHandler<UnsubscribeFromDevicePacket>
{
    protected override void ChannelRead0(IChannelHandlerContext ctx, UnsubscribeFromDevicePacket msg)
    {
        RemoteServer serv = ctx.Channel.GetAttribute(ServerAttributes.ServerAttribute).Get();

        if (!serv.Devices.TryGetValue(msg.DeviceIdentifier, out RemoteDevice? device))
        {
            Log.Warning("Received device unsubscribe packet for nonexistent device '{Identifier}'", msg.DeviceIdentifier);
            return;
        }

        Log.Information("Received device unsubscribe packet for device '{Identifier}'", msg.DeviceIdentifier);
        _ = device.UnsubscribeAsync(false);
    }
}