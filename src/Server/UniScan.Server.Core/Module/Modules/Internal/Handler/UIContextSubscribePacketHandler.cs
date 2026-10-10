using DotNetty.Transport.Channels;
using Serilog;
using Shiki.Common.Result;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.Packets.Serverbound.SSR;
using UniScan.Server.Core.UserInterface;

namespace UniScan.Server.Core.Module.Modules.Internal.Handler;

public class UIContextSubscribePacketHandler(IUISSRManager ssrManager)
    : SimpleChannelInboundHandler<UIContextSubscribePacket>
{
    private readonly IUISSRManager _ssrManager = ssrManager;
    private readonly IUISSRManagerMutationProxy _ssrManagerMutationProxy = ssrManager;

    private readonly ILogger _logger = Log.ForContext<UIContextSubscribePacketHandler>();

    protected override void ChannelRead0(IChannelHandlerContext ctx, UIContextSubscribePacket msg)
    {
        if (!_ssrManager.SessionFactories.TryGetValue(msg.Identifier, out IUISSRContextSessionFactory? factory))
        {
            _logger.Warning("Client tried to subscribe to nonexistent session '{Identifier}'", msg.Identifier);
            
            ctx.WriteAndFlushAsync(new AcknowledgePacket(new TransportableBooleanResult(new KeyNotFoundException("Identifier was not found.")), msg.RequestId));
            return;
        }

        if (!factory.SupportsArgs(msg.Args))
        {
            _logger.Warning("Client tried to subscribe to session '{Identifier}', but sent invalid args (of type: {Type}).", msg.Identifier, msg.Args?.GetType());
            
            ctx.WriteAndFlushAsync(new AcknowledgePacket(new TransportableBooleanResult(new NotSupportedException("Factory does not support provided args.")), msg.RequestId));
            return;
        }
        
        try
        {
            IUISSRContextSession session = factory.Create(ctx.Channel, msg.Identifier, msg.Args);
            _ssrManagerMutationProxy.Subscribe(ctx.Channel, session);

            session.InitializeAsync();
            _logger.Information("Client subscribed to session '{Identifier}'", msg.Identifier);
            
            ctx.WriteAndFlushAsync(new AcknowledgePacket(TransportableBooleanResult.Successful, msg.RequestId));
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to subscribe client to session '{Identifier}'", msg.Identifier);
            ctx.WriteAndFlushAsync(new AcknowledgePacket(new TransportableBooleanResult(new NotSupportedException("An exception was thrown while setting up the UI context session.")), msg.RequestId));
        }
    }
}