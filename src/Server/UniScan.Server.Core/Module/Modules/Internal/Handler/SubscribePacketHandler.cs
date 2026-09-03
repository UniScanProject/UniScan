using System.ComponentModel.Design;
using System.Numerics;
using DotNetty.Transport.Channels;
using Serilog;
using Shiki.Common.Extensions;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Result;
using Shiki.Common.Result.Serialization.Types;
using Shiki.Common.Util;
using UniScan.Core.State;
using UniScan.Core.State.Node;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.Packets.Clientbound.Device;
using UniScan.Network.Protocol.Packets.Clientbound.SSR;
using UniScan.Network.Protocol.Packets.Serverbound.Subscription;
using UniScan.Server.Core.Host;
using UniScan.UserInterface.Definitions;

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

        if (_scannerHostManager.Scanners.TryGetValue(msg.DeviceIdentifier, out ScannerHost? host))
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
                ctx.WriteAsync(new DeviceStatePacket(DeviceStateSerializer.Serialize(host.Scanner.State.Value), msg.RequestId, host.Identifier));
            }

            var id = new Identifier("UniScan", "ssr", "slot", "device", msg.DeviceIdentifier);
            ctx.WriteAsync(new SetUISlotPacket(id, new ContainerUIControl(
                                                                          new TextBlockUIControl("Hello, world 1!")
                                                                          {
                                                                              FontSize = 16
                                                                          },
                                                                          new TextBlockUIControl("Hello, world 2!"),
                                                                          new TextBlockUIControl("Hello, world 3!"),
                                                                          new TextBlockUIControl("Hello, world 4!")
                                                                         )
            {
                Id = "parent".ToSlug<DashSlugFormatter>(),
                Style =
                {
                    Padding = new Vector4(10),
                    Position =
                    {
                        HorizontalPosition = HorizontalPosition.Center
                    }
                }
            }));

            ctx.Flush();

            host.NetworkClients.AddClient(ctx.Channel);
        }
        else
        {
            _logger.Warning("Client attempted to subscribe to nonexistent device '{Value}'. Discarding.", msg.DeviceIdentifier);
            
            ctx.WriteAndFlushAsync(new
                                       AcknowledgePacket(new
                                                                 BooleanResult<
                                                                     Exception>(new
                                                                                    KeyNotFoundException("Scanner not found"))
                                                            .GetTransportableResult(), msg.RequestId));
        }
    }
}