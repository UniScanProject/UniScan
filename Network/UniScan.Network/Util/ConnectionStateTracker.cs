using DotNetty.Transport.Channels;

namespace UniScan.Network.Util;

public class ConnectionStateTracker : ChannelHandlerAdapter
{
    public override bool IsSharable { get; } = true;
    
    public class ConnectionStateChangedEventArgs(IChannel channel) : EventArgs
    {
        public IChannel Channel { get; } = channel;
    }

    public event EventHandler<ConnectionStateChangedEventArgs>? Connected;
    public event EventHandler<ConnectionStateChangedEventArgs>? Disconnected;
    
    public override void ChannelActive(IChannelHandlerContext context)
    {
        Connected?.Invoke(this, new ConnectionStateChangedEventArgs(context.Channel));
        
        base.ChannelActive(context);
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        Disconnected?.Invoke(this, new ConnectionStateChangedEventArgs(context.Channel));
        
        base.ChannelInactive(context);
    }
}