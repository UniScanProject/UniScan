using DotNetty.Transport.Channels;
using Shiki.Common.Identity;
using Shiki.Common.Result;
using UniScan.Network.Data;
using UniScan.Network.Packet.Packets.Clientbound;
using UniScan.Network.Packet.Packets.Serverbound;
using UniScan.Server.Core.Host;

namespace UniScan.Server.Core.Module.Modules.Internal.Handler;

/// <summary>
/// Handles incoming GetDeviceList, sent by clients to retrieve the device list
/// </summary>
/// <param name="scannerHostManager">The host manager</param>
public class GetDeviceListPacketHandler(ScannerHostManager scannerHostManager) : SimpleChannelInboundHandler<GetDeviceListPacket>
{
    private readonly ScannerHostManager _scannerHostManager = scannerHostManager;

    protected override void ChannelRead0(IChannelHandlerContext ctx, GetDeviceListPacket msg)
    {
        if (msg.RequestId == null)
            return;

        var devices = _scannerHostManager.Scanners.ToDictionary(i => i.Key, h =>
        {
            DeviceSpecifications? info = null;
            if (h.Value.Scanner.ScannerInfo != null)
            {
                info = new DeviceSpecifications(h.Value.Scanner.ScannerInfo.Model, h.Value.Scanner.ScannerInfo.Version);
            }

            return new DeviceDto(h.Key, h.Value.DisplayName, h.Value.Scanner.Active, info);
        });
        ctx.WriteAndFlushAsync(new DeviceListPacket(devices, msg.RequestId));
    }
}