using System;
using DotNetty.Transport.Channels;
using UniScan.Client.Core.SSR;
using UniScan.Network.Protocol.Packets.Clientbound.SSR;

namespace UniScan.Client.Core.Module.Modules.Internal.Handler;


public class SetUINodePacketHandler(IUISlotRegistry registry) : SimpleChannelInboundHandler<SetUINodePacket>
{
    protected override void ChannelRead0(IChannelHandlerContext ctx, SetUINodePacket msg)
    {
        SSRSlotContext? vm = registry.Get(msg.SlotIdentifier);
        if (vm == null)
            throw new Exception($"UI slot {msg.SlotIdentifier} not found");
    
        if (vm.NodeMap.TryGetValue(msg.NodeIdentifier, out var node))
        {
            node.Value = msg.Node;
        }
    }
}