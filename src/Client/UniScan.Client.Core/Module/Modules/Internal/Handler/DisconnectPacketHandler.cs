using DotNetty.Transport.Channels;
using Serilog;
using Shiki.Internationalization;
using Shiki.Internationalization.Component;
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
        ITextComponent reason = msg.Reason ?? new TranslatableTextComponent("UniScan:client/i18n/disconnect_reason/unknown");
        
        _logger.Information("Disconnected by server {ChannelIp}: {Information}", ctx.Channel.RemoteAddress, reason);
        RemoteServer server = ctx.Channel.GetAttribute(ServerAttributes.ServerAttribute).Get();

        if (server.ConnectionStatus.Value.State != ConnectionState.UserDisconnected && server.ConnectionStatus.Value.State != ConnectionState.UnexpectedDisconnected)
        {
            ((IRemoteServerMutationProxy)server)
               .SetConnectionStatus(new KickedDisconnectedConnectionStatusContext(reason));
        }

        ctx.CloseAsync();
    }
}