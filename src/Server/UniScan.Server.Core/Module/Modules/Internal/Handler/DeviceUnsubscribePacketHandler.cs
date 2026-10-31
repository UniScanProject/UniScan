using DotNetty.Transport.Channels;
using Shiki.Common.Identity;
using Shiki.Common.Result;
using UniScan.Network.Data;
using UniScan.Network.Data.Device;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.Packets.Clientbound;
using UniScan.Network.Protocol.Packets.Serverbound;
using UniScan.Network.Protocol.Packets.Serverbound.Device;
using UniScan.Server.Core.Host;

namespace UniScan.Server.Core.Module.Modules.Internal.Handler;

public class DeviceUnsubscribePacketHandler(DeviceHostManager deviceHostManager) : SimpleChannelInboundHandler<DeviceUnsubscribePacket>
{
    private readonly DeviceHostManager _deviceHostManager = deviceHostManager;

    protected override void ChannelRead0(IChannelHandlerContext ctx, DeviceUnsubscribePacket msg)
    {
        if (msg.RequestId == null)
            return;

        if (!_deviceHostManager.Devices.TryGetValue(msg.DeviceIdentifier, out DeviceHost? device))
        {
            //TODO codegen to generate WriteAndFlushPacketAsync for each packet variation
            ctx.WriteAndFlushAsync(new AcknowledgePacket(new BooleanResult<Exception>(new KeyNotFoundException()),
                                                         msg.RequestId));
            return;
        }

        if (!device.NetworkClients.Contains(ctx.Channel))
        {
            ctx.WriteAndFlushAsync(new AcknowledgePacket(new BooleanResult<Exception>(new InvalidOperationException("Not subscribed to this device")), msg.RequestId));
            return;
        }
        
        device.NetworkClients.RemoveClient(ctx.Channel);
        ctx.WriteAndFlushAsync(new AcknowledgePacket(TransportableBooleanResult.Successful, msg.RequestId));
    }
}