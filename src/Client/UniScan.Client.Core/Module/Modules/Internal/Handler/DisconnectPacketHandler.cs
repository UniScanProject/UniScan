using DotNetty.Transport.Channels;
using Serilog;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Connection;
using UniScan.Client.Core.Remote.Connection.Status;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;

namespace UniScan.Client.Core.Module.Modules.Internal.Handler;

public class DisconnectPacketHandler : SimpleChannelInboundHandler<DisconnectPacket>
{
    private readonly ILogger _logger = Log.ForContext<DisconnectPacketHandler>();
    
    protected override void ChannelRead0(IChannelHandlerContext ctx, DisconnectPacket msg)
    {
        string reason = msg.Reason ?? "Unknown reason, maybe the server sent back a malformed DisconnectPacket?";
        
        _logger.Information("Disconnected by server {ChannelIp}: {Information}", ctx.Channel.RemoteAddress, reason);
        RemoteServer server = ctx.Channel.GetAttribute(ServerAttributes.ServerAttribute).Get();

        if (server.ConnectionStatus.Value.State != ConnectionState.UserDisconnected)
        {
            ((IRemoteServerMutationProxy)server)
               .SetConnectionStatus(new KickedDisconnectedConnectionStatusContext(reason));
        }

        ctx.CloseAsync();
    }
}