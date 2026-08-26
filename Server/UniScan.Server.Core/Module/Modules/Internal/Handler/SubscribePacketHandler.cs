using System.ComponentModel.Design;
using DotNetty.Transport.Channels;
using Shiki.Common.Result;
using Shiki.Common.Util;
using UniScan.Network.Packet.Packets.Bidirectional.Status;
using UniScan.Network.Packet.Packets.Clientbound.Device;
using UniScan.Network.Packet.Packets.Serverbound.Subscription;
using UniScan.Server.Core.Host;

namespace UniScan.Server.Core.Module.Modules.Internal.Handler;

/// <summary>
/// Handles incoming SubscribePackets, used for clients to subscribe to and receive updates on a scanner
/// </summary>
/// <param name="scannerHostManager">The host manager</param>
public class SubscribePacketHandler(ScannerHostManager scannerHostManager) : SimpleChannelInboundHandler<SubscribePacket>
{
    private readonly ScannerHostManager _scannerHostManager = scannerHostManager;

    protected override void ChannelRead0(IChannelHandlerContext ctx, SubscribePacket msg)
    {
        if (msg.RequestId == null)
            return;
        
        if (_scannerHostManager.Scanners.TryGetValue(msg.ScannerIdentifier, out ScannerHost? host))
        {
            ctx.WriteAsync(new AcknowledgePacket(new TransportableBooleanResult(null), msg.RequestId));
            
            // ctx.WriteAsync(new ScannerRegistrationPacket(host.DisplayName, msg.RequestId, msg.ScannerIdentifier));
            if (host.Scanner.State.Value != null)
            {
                ctx.WriteAsync(new StatePacket(host.Scanner.State.Value, msg.RequestId, host.Identifier));
            }

            ctx.Flush();
            
            host.NetworkClients.AddClient(ctx.Channel);
        }
        else
        {
            ctx.WriteAndFlushAsync(new AcknowledgePacket(new BooleanResult<Exception>(new KeyNotFoundException("Scanner not found")).GetTransportableResult(), msg.RequestId));
        }
    }
}