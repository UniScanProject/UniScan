using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;

namespace UniScan.Network.Server.Handler;

public class ClientsManager : ChannelHandlerAdapter
{
    private readonly SubscribableGroup _channels = new();
    public override bool IsSharable => true;

    public override void ChannelActive(IChannelHandlerContext context)
    {
        _channels.Add(context.Channel);
        base.ChannelActive(context);
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        _channels.Remove(context.Channel);
        base.ChannelInactive(context);
    }

    public async Task BroadcastAsync(IPacket packet)
    {
        await _channels.BroadcastAsync(packet);
    }
}