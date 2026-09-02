using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Serilog;
using Serilog.Core;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Server;

namespace UniScan.Server.Core.Module.Modules.Internal.Filter;

public class AcceptedClientPacketFilter : ChannelHandlerAdapter
{
    public override bool IsSharable => true;
    private ILogger _logger = Log.ForContext<AcceptedClientPacketFilter>();

    /// <inheritdoc/>
    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        if (message is IRequiresAcceptedClientPayloadPart)
        {
            if (!context.Channel.HasAttribute(ClientAttributes.SoftwareInfoAttribute))
            {
                _logger.Error("Client '{RemoteAddress}' sent packet of type '{Type}' without completing handshake.", context.Channel.RemoteAddress, message.GetType().FullName);
                ReferenceCountUtil.Release(message);
                
                context.WriteAndFlushAsync(new DisconnectPacket("Client has not completed handshake.")).ContinueWith(_ => context.CloseAsync());
                return;
            }
        }
        
        base.ChannelRead(context, message);
    }
}