using System;
using DotNetty.Transport.Channels;
using UniScan.Client.Core.SSR;
using UniScan.Network.Protocol.Packets.Clientbound.SSR;

namespace UniScan.Client.Core.Module.Modules.Internal.Handler;


public class SetUISlotPacketHandler(IUISlotRegistry registry) : SimpleChannelInboundHandler<SetUISlotPacket>
{
    protected override void ChannelRead0(IChannelHandlerContext ctx, SetUISlotPacket msg)
    {
        SSRSlotContext? vm = registry.Get(msg.SlotIdentifier);
        if (vm == null)
            throw new Exception($"UI slot {msg.SlotIdentifier} not found");
 
        //clear existing map (rebuilding...)
        vm.NodeMap.Clear();

        //set
        vm.Node.Value = msg.Node;
    }
}