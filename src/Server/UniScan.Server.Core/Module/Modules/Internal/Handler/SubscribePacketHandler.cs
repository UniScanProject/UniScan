using System.Drawing;
using System.Numerics;
using DotNetty.Transport.Channels;
using Serilog;
using Shiki.Common.Extensions;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Result;
using UniScan.Core.State;
using UniScan.Core.State.Node;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.Packets.Clientbound.Device;
using UniScan.Network.Protocol.Packets.Clientbound.SSR;
using UniScan.Network.Protocol.Packets.Serverbound.Subscription;
using UniScan.Server.Core.Host;

namespace UniScan.Server.Core.Module.Modules.Internal.Handler;

/// <summary>
/// Handles incoming SubscribePackets, used for clients to subscribe to and receive updates on a scanner
/// </summary>
/// <param name="deviceHostManager">The host manager</param>
public class SubscribePacketHandler(DeviceHostManager deviceHostManager)
    : SimpleChannelInboundHandler<DeviceSubscribePacket>
{
    private readonly DeviceHostManager _deviceHostManager = deviceHostManager;
    private readonly ILogger _logger = Log.ForContext<SubscribePacketHandler>();

    protected override void ChannelRead0(IChannelHandlerContext ctx, DeviceSubscribePacket msg)
    {
        if (msg.RequestId == null)
            return;

        if (_deviceHostManager.Devices.TryGetValue(msg.DeviceIdentifier, out DeviceHost? host))
        {
            if (host.NetworkClients.Contains(ctx.Channel))
            {
                _logger.Warning("Client attempted to subscribe to already subscribed device. Discarding.");

                ctx.WriteAndFlushAsync(
                                       new AcknowledgePacket(
                                                             new BooleanResult<InvalidOperationException>(
                                                                  new InvalidOperationException("You are already subscribed to this device.")
                                                                 ).ToTransportableBooleanResult(),
                                                             msg.RequestId)
                                      );
                return;
            }

            if (!host.Device.Active.Value)
            {
                _logger.Warning("Client attempted to subscribe to inactive device. Discarding.");

                ctx.WriteAndFlushAsync(
                                       new AcknowledgePacket(
                                                             new BooleanResult<InvalidOperationException>(
                                                                  new InvalidOperationException("This device is not active.")
                                                                 ).ToTransportableBooleanResult(),
                                                             msg.RequestId)
                                      );
                return;
            }

            ctx.WriteAsync(new AcknowledgePacket(TransportableBooleanResult.Successful, msg.RequestId));
            if (host.Device.State.Value != null)
            {
                ctx.WriteAsync(new StatePacket(StateSerializer.SerializeContainer(host.Device.State.Value).ToDictionary(kvp => kvp.Key, IStateNode? (kvp) => kvp.Value), msg.RequestId, host.Identifier));
            }

            // var id = new Identifier("UniScan", "ssr", "slot", "device", msg.DeviceIdentifier);
            // ctx.WriteAsync(new SetUISlotPacket(id, new ContainerUIControl(
            //                                                               new TextBlockUIControl("Hello, world 1!")
            //                                                               {
            //                                                                   FontSize = 16
            //                                                               },
            //                                                               new TextBlockUIControl("Hello, world 2!"),
            //                                                               new TextBlockUIControl("Hello, world 3!"),
            //                                                               new TextBlockUIControl("Hello, world 4!"),
            //                                                               new BoundTextBlockUIControl(new Identifier("UniScan", "garbage"))
            //                                                              )
            // {
            //     Id = "parent".ToSlug<DashSlugFormatter>(),
            //     Style =
            //     {
            //         CornerRadius = new Vector4(10),
            //         BackgroundColor = Color.LightGray,
            //         Padding = new Vector4(10),
            //         Margin = new Vector4(10),
            //         Position =
            //         {
            //             HorizontalPosition = HorizontalPosition.Center,
            //             VerticalPosition = VerticalPosition.Top
            //         }
            //     }
            // }));

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
                                                                                    KeyNotFoundException("Device not found"))
                                                            .ToTransportableBooleanResult(), msg.RequestId));
        }
    }
}