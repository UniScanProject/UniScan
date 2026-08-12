using DotNetty.Transport.Channels;
using Shiki.Common.Identity;
using Shiki.Common.Result;
using UniScan.Network.Packet.Packets.Clientbound;
using UniScan.Network.Packet.Packets.Clientbound.Remote;
using UniScan.Network.Packet.Packets.Serverbound.Server;
using UniScan.Server.Core.Host;

namespace UniScan.Server.Core.Module.Modules.Internal.Handler;

/// <summary>
/// Handles incoming GetRemoteInfoPacket, sent by clients to get metadata associated with this Remote
/// </summary>
public class GetRemoteInfoPacketHandler : SimpleChannelInboundHandler<GetRemoteInfoPacket>
{
    protected override void ChannelRead0(IChannelHandlerContext ctx, GetRemoteInfoPacket msg)
    {
        if (msg.RequestId == null)
            return;
        
        // ctx.WriteAndFlushAsync(new RemoteInfoPacket());
    }
}