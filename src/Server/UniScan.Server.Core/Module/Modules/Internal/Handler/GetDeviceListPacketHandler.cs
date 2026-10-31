using DotNetty.Transport.Channels;
using Shiki.Common.Identity;
using Shiki.Common.Result;
using UniScan.Network.Data;
using UniScan.Network.Data.Device;
using UniScan.Network.Protocol.Packets.Clientbound;
using UniScan.Network.Protocol.Packets.Serverbound;
using UniScan.Server.Core.Host;
using UniScan.Server.Core.Util;

namespace UniScan.Server.Core.Module.Modules.Internal.Handler;

/// <summary>
/// Handles incoming GetDeviceList, sent by clients to retrieve the device list
/// </summary>
/// <param name="deviceHostManager">The host manager</param>
public class GetDeviceListPacketHandler(DeviceHostManager deviceHostManager) : SimpleChannelInboundHandler<GetDeviceListPacket>
{
    private readonly DeviceHostManager _deviceHostManager = deviceHostManager;

    protected override void ChannelRead0(IChannelHandlerContext ctx, GetDeviceListPacket msg)
    {
        if (msg.RequestId == null)
            return;

        var devices = _deviceHostManager.Devices.ToDictionary(i => i.Key, h => DeviceDto.FromDeviceHost(h.Value));
        
        ctx.WriteAndFlushAsync(new DeviceListPacket(devices, msg.RequestId));
    }
}