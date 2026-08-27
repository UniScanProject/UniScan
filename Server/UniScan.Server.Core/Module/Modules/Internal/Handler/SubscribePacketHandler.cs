using System.ComponentModel.Design;
using DotNetty.Transport.Channels;
using Serilog;
using Shiki.Common.Result;
using Shiki.Common.Result.Serialization.Types;
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
public class SubscribePacketHandler(ScannerHostManager scannerHostManager)
    : SimpleChannelInboundHandler<SubscribePacket>
{
    private readonly ScannerHostManager _scannerHostManager = scannerHostManager;
    private readonly ILogger _logger = Log.ForContext<SubscribePacketHandler>();

    protected override void ChannelRead0(IChannelHandlerContext ctx, SubscribePacket msg)
    {
        if (msg.RequestId == null)
            return;

        if (_scannerHostManager.Scanners.TryGetValue(msg.ScannerIdentifier, out ScannerHost? host))
        {
            if (host.NetworkClients.Contains(ctx.Channel))
            {
                _logger.Warning("Client attempted to subscribe to already subscribed device. Discarding.");

                ctx.WriteAndFlushAsync(
                                       new AcknowledgePacket(
                                                             new BooleanResult<InvalidOperationException>(
                                                                  new InvalidOperationException("You are already subscribed to this device.")
                                                                 ).GetTransportableResult(),
                                                             msg.RequestId)
                                      );
                return;
            }


            ctx.WriteAsync(new AcknowledgePacket(new TransportableBooleanResult(null), msg.RequestId));
            if (host.Scanner.State.Value != null)
            {
                ctx.WriteAsync(new StatePacket(host.Scanner.State.Value, msg.RequestId, host.Identifier));
            }

            ctx.Flush();

            host.NetworkClients.AddClient(ctx.Channel);
        }
        else
        {
            _logger.Warning("Client attempted to subscribe to nonexistent device '{Value}'. Discarding.", msg.ScannerIdentifier);
            
            ctx.WriteAndFlushAsync(new
                                       AcknowledgePacket(new
                                                                 BooleanResult<
                                                                     Exception>(new
                                                                                    KeyNotFoundException("Scanner not found"))
                                                            .GetTransportableResult(), msg.RequestId));
        }
    }
}