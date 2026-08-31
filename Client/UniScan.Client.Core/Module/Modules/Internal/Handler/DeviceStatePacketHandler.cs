using DotNetty.Transport.Channels;
using ObservableCollections;
using Serilog;
using Shiki.Common.Identity;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Remote.Device;
using UniScan.Core.State;
using UniScan.Core.State.Node;
using UniScan.Network.Packet.Packets.Clientbound.Device;

namespace UniScan.Client.Core.Module.Modules.Internal.Handler;

public class DeviceStatePacketHandler : SimpleChannelInboundHandler<DeviceStatePacket>
{
    protected override void ChannelRead0(IChannelHandlerContext ctx, DeviceStatePacket msg)
    {
        RemoteServer serv = ctx.Channel.GetAttribute(ServerAttributes.ServerAttribute).Get();
        if (!serv.Devices.TryGetValue(msg.ScannerIdentifier, out RemoteDevice? device))
            return; //todo disconnect

        device.States = new ObservableDictionary<Identifier, IDeviceStateNode>(msg.States);
        Log.Information("Received states: ");
        foreach (var state in device.States)
        {
            Log.Information("[{Key}: {Value}]", state.Key, state.Value.ToString());
        }
    }
}