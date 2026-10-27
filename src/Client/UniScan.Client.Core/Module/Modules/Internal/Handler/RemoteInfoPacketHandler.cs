using DotNetty.Transport.Channels;
using Serilog;
using UniScan.Client.Core.Remote;
using UniScan.Network.Protocol.Packets.Clientbound.Remote;

namespace UniScan.Client.Core.Module.Modules.Internal.Handler;

public class RemoteInfoPacketHandler : SimpleChannelInboundHandler<RemoteInfoPacket>
{
    private readonly ILogger _logger = Log.ForContext<RemoteInfoPacketHandler>();
    
    protected override void ChannelRead0(IChannelHandlerContext ctx, RemoteInfoPacket msg)
    {
        _logger.Information("Received remote info: {Information}", msg.Info);
      
        //TODO reimpl as RemoteSettingsPacket
        // #if DEBUG
        // Log.Debug("Server exposes the following settings: ");
        // foreach (var kvp in msg.Info.Settings)
        // {
        //     Log.Debug("  - {Key} -> {Value}", kvp.Key, kvp.Value);
        // }
        // #endif
        
        RemoteServer serv = ctx.Channel.GetAttribute(ServerAttributes.ServerAttribute).Get();
        ((IRemoteServerMutationProxy)serv).SetRemoteInfo(msg.Info);
    }
}