using DotNetty.Transport.Channels;
using Serilog;
using Shiki.Common.Result;
using Shiki.Internationalization.Component;
using Shiki.ModuleManagement;
using UniScan.Network;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.Packets.Clientbound.Remote;
using UniScan.Network.Protocol.Packets.Serverbound.Client;
using UniScan.Network.Server;
using UniScan.Server.Core.Social;

namespace UniScan.Server.Core.Module.Modules.Internal.Handler;

public class ClientSoftwareInfoPacketHandler(RemoteInfoManager remoteInfoManager, ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs> moduleStorage) : SimpleChannelInboundHandler<ClientSoftwareInfoPacket>
{
    private ILogger _logger = Log.ForContext<ClientSoftwareInfoPacketHandler>();
    
    protected override void ChannelRead0(IChannelHandlerContext ctx, ClientSoftwareInfoPacket msg)
    {
        if (msg.RequestId == null)
            return;

        _logger.Information("Received client information from channel {ChannelIp} ({Name}): {Information}", ctx.Channel.RemoteAddress, ctx.Name, msg.Info);
        
        if (ctx.Channel.HasAttribute(ClientAttributes.SoftwareInfoAttribute))
        {
            _logger.Information("Disconnecting client {ChannelIp} as they sent client info after already completing handshake", ctx.Channel.RemoteAddress);

            ctx.WriteAndFlushAsync(new DisconnectPacket(new TranslatableTextComponent("UniScan:i18n/disconnect/handshake_already_completed"))).ContinueWith(_ => ctx.CloseAsync());
            return;
        }

        if (msg.Info.ProtocolVersion != Network.Constants.ProtocolVersion)
        {
            _logger.Information("Disconnecting client {ChannelIp} due to protocol version mismatch. (expected: {ServerProtocolVer}, got: {ProtocolVer})", ctx.Channel.RemoteAddress, Network.Constants.ProtocolVersion, msg.Info.ProtocolVersion);

            ctx.WriteAndFlushAsync(new DisconnectPacket(new TranslatableTextComponent("UniScan:i18n/disconnect/mismatching_protocol_version", msg.Info.ProtocolVersion, Network.Constants.ProtocolVersion))).ContinueWith(_ => ctx.CloseAsync());
            return;
        }
        
        ctx.Channel.GetAttribute(ClientAttributes.SoftwareInfoAttribute).Set(msg.Info);

        ctx.WriteAndFlushAsync(new ServerSoftwareInfoPacket(UniScanServer.SoftwareInfo, moduleStorage.Modules.ToDictionary(m => m.Key, m => new ModuleInfo(m.Value.Id)), msg.RequestId));
        ctx.WriteAndFlushAsync(new RemoteInfoPacket(remoteInfoManager.GetRemoteInfo()));
    }
}