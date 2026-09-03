using DotNetty.Transport.Channels;
using Serilog;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;

namespace UniScan.Server.Core.Module.Modules.Internal.Handler;

public class DisconnectPacketHandler : SimpleChannelInboundHandler<DisconnectPacket>
{
    private ILogger _logger = Log.ForContext<DisconnectPacketHandler>();
    
    protected override void ChannelRead0(IChannelHandlerContext ctx, DisconnectPacket msg)
    {
        _logger.Information("Client {Address} disconnected self with reason '{Reason}'", ctx.Channel.RemoteAddress, msg.Reason);
        ctx.WriteAndFlushAsync(new DisconnectPacket($"Client initiated disconnect with reason '{msg.Reason}'")).ContinueWith(_ => ctx.CloseAsync());
    }
}