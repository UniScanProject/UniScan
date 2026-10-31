using DotNetty.Transport.Channels;
using ObservableCollections;
using Serilog;
using Shiki.Common.Identity;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Device;
using UniScan.Core.State;
using UniScan.Core.State.Node;
using UniScan.Network.Protocol.Packets.Clientbound.Device;

namespace UniScan.Client.Core.Module.Modules.Internal.Handler;

public class DeviceStatePacketHandler : SimpleChannelInboundHandler<StatePacket>
{
    protected override void ChannelRead0(IChannelHandlerContext ctx, StatePacket msg)
    {
        RemoteServer serv = ctx.Channel.GetAttribute(ServerAttributes.ServerAttribute).Get();
        if (!serv.Devices.TryGetValue(msg.DeviceIdentifier, out RemoteDevice? device))
            return; //todo disconnect

        foreach (var state in msg.States)
        {
            if (state.Value is null)
            {
                device.States.Remove(state.Key);
                continue;
            }

            device.States[state.Key] = state.Value;
        }
    }
}