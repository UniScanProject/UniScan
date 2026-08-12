using DotNetty.Transport.Channels;
using UniScan.Network.Packet.PayloadPart;

namespace UniScan.Network.Request;

public class ResponseHandler(RequestManager mgr) : ChannelHandlerAdapter
{
    /// <inheritdoc/>
    public override void ChannelRead(IChannelHandlerContext ctx, object msg)
    {
        if (msg is IPacket packet and IRequestIdPayloadPart && mgr.TryCompleteRequest(packet))
        {
            return;
        }

        ctx.FireChannelRead(msg);
    }
}