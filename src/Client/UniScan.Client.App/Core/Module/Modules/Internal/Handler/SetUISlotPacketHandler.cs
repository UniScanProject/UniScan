using System;
using Avalonia.Threading;
using DotNetty.Transport.Channels;
using Serilog;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.App.Views.SSR;
using UniScan.Client.Core.Module.Modules.Internal.Handler;
using UniScan.Client.Core.Remote;
using UniScan.Network.Protocol.Packets.Clientbound.Remote;
using UniScan.Network.Protocol.Packets.Clientbound.SSR;

namespace UniScan.Client.App.Core.Module.Modules.Internal.Handler;


public class SetUISlotPacketHandler(IUISlotRegistry registry, IUIViewFactory factory) : SimpleChannelInboundHandler<SetUISlotPacket>
{
    private readonly ILogger _logger = Log.ForContext<SetUISlotPacketHandler>();
    
    protected override void ChannelRead0(IChannelHandlerContext ctx, SetUISlotPacket msg)
    {
        try
        {
            IUISlotControlViewModel? vm = registry.Get(msg.SlotIdentifier);
            if (vm == null)
                throw new Exception($"UI slot {msg.SlotIdentifier} not found");
        
            if (msg.Node == null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    vm.Node = null;
                });
                _logger.Debug("Removed slot {Slot}", msg.SlotIdentifier);
            
                return;
            }
            
            Dispatcher.UIThread.Post(() =>
            {
                object view = factory.CreateView(msg.Node);
                vm.Node = view;
            });
            _logger.Debug("Set slot {Slot}", msg.SlotIdentifier);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to process SSR components provided by server");
        }
    }
}