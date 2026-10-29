#if UNISCAN_CLIENT
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Transport.Channels;

namespace UniScan.Modules.WebSocketTransportMethod.Network;

public class WebSocketHandshakeAwaiter : ChannelHandlerAdapter
{
    private readonly TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Task Task => _tcs.Task;

    public override void UserEventTriggered(IChannelHandlerContext context, object evt)
    {
        if (evt is WebSocketClientProtocolHandler.ClientHandshakeStateEvent.HandshakeComplete)
        {
            _tcs.TrySetResult(true);
        }
        
        base.UserEventTriggered(context, evt);
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        _tcs.TrySetException(exception);
        base.ExceptionCaught(context, exception);
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        _tcs.TrySetCanceled();
        base.ChannelInactive(context);
    }
}
#endif