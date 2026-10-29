using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Transport.Channels;

namespace UniScan.Modules.WebSocketTransportMethod.Network;

public class WebSocketFrameAdapter : MessageToMessageCodec<WebSocketFrame, IByteBuffer>
{
    protected override void Encode(IChannelHandlerContext ctx, IByteBuffer msg, List<object> output)
    {
        output.Add(new BinaryWebSocketFrame(msg.RetainedSlice()));
    }

    protected override void Decode(IChannelHandlerContext ctx, WebSocketFrame msg, List<object> output)
    {
        output.Add(msg.Content.RetainedSlice());
    }
}