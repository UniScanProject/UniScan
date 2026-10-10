using DotNetty.Transport.Channels;
using Serilog;
using Shiki.Common.Result;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.Packets.Serverbound.SSR;
using UniScan.Server.Core.UserInterface;

namespace UniScan.Server.Core.Module.Modules.Internal.Handler;

public class UIContextUnsubscribePacketHandler(IUISSRManager ssrManager)
    : SimpleChannelInboundHandler<UIContextUnsubscribePacket>
{
    private readonly IUISSRManager _ssrManager = ssrManager;
    private readonly IUISSRManagerMutationProxy _ssrManagerMutationProxy = ssrManager;

    private readonly ILogger _logger = Log.ForContext<SubscribePacketHandler>();

    protected override void ChannelRead0(IChannelHandlerContext ctx, UIContextUnsubscribePacket msg)
    {
        if (!_ssrManager.SessionFactories.TryGetValue(msg.Identifier, out _))
        {
            ctx.WriteAndFlushAsync(new AcknowledgePacket(new TransportableBooleanResult(new KeyNotFoundException("Identifier was not found."))));
            return;
        }

        _ssrManagerMutationProxy.Unsubscribe(ctx.Channel, msg.Identifier);
        
        ctx.WriteAndFlushAsync(AcknowledgePacket.Success);
    }
}