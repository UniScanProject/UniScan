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

public class DeviceInfoPacketHandler : SimpleChannelInboundHandler<DeviceInfoPacket>
{
    protected override void ChannelRead0(IChannelHandlerContext ctx, DeviceInfoPacket msg)
    {
        RemoteServer serv = ctx.Channel.GetAttribute(ServerAttributes.ServerAttribute).Get();

        Log.Information("Received device info for {Identifier}: {Device}", msg.Device.DeviceIdentifier, msg.Device);
        if (!serv.Devices.TryGetValue(msg.DeviceIdentifier, out RemoteDevice? device))
        {
            serv.Devices[msg.DeviceIdentifier] = RemoteDevice.FromDto(msg.Device, serv);
            return;
        }
        
        //TODO better way than this to apply diffs?
        if (!msg.Device.Connected && device.Subscribed.Value)
        {
            _ = device.UnsubscribeAsync(true);
        }
        
        device.UpdateFrom(msg.Device);
    }
}