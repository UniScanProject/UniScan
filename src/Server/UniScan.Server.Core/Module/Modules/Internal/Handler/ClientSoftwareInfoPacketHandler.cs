using DotNetty.Transport.Channels;
using Serilog;
using Shiki.Common.Result;
using UniScan.Network;
using UniScan.Network.Packet.Packets.Bidirectional.Status;
using UniScan.Network.Packet.Packets.Clientbound.Remote;
using UniScan.Network.Packet.Packets.Serverbound.Client;
using UniScan.Network.Server;

namespace UniScan.Server.Core.Module.Modules.Internal.Handler;

public class ClientSoftwareInfoPacketHandler : SimpleChannelInboundHandler<ClientSoftwareInfoPacket>
{
    private ILogger _logger = Log.ForContext<ClientSoftwareInfoPacketHandler>();
    
    protected override void ChannelRead0(IChannelHandlerContext ctx, ClientSoftwareInfoPacket msg)
    {
        if (msg.RequestId == null)
            return;

        _logger.Information("Received client information from channel {ChannelIp}: {Information}", ctx.Channel.RemoteAddress, msg.Info);
        
        if (ctx.Channel.HasAttribute(ClientAttributes.SoftwareInfoAttribute))
        {
            _logger.Information("Disconnecting client {ChannelIp} as they sent client info after already completing handshake.", ctx.Channel.RemoteAddress);

            ctx.WriteAndFlushAsync(new DisconnectPacket("Client has already completed software info handshake")).ContinueWith(_ => ctx.CloseAsync());
            return;
        }

        if (msg.Info.ProtocolVersion != Constants.ProtocolVersion)
        {
            _logger.Information("Disconnecting client {ChannelIp} due to protocol version mismatch. (expected: {ServerProtocolVer}, got: {ProtocolVer})", ctx.Channel.RemoteAddress, Constants.ProtocolVersion, msg.Info.ProtocolVersion);

            ctx.WriteAndFlushAsync(new DisconnectPacket($"Client protocol version '{msg.Info.ProtocolVersion}' does not match expected '{Constants.ProtocolVersion}'")).ContinueWith(_ => ctx.CloseAsync());
            return;
        }
        
        ctx.Channel.GetAttribute(ClientAttributes.SoftwareInfoAttribute).Set(msg.Info);

        ctx.WriteAndFlushAsync(new ServerSoftwareInfoPacket(UniScanServer.SoftwareInfo, msg.RequestId));
        ctx.WriteAndFlushAsync(new RemoteInfoPacket(UniScanServer.RemoteInfo));
    }
}