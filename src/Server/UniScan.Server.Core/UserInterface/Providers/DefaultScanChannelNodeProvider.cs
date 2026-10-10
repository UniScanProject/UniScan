using System.Numerics;
using Shiki.Common.Identity;
using Shiki.Internationalization.Component;
using UniScan.Core.State.Radio;
using UniScan.Network;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;
using UniScan.UserInterface.Properties.Styling;
using ContainerUIControl = UniScan.UserInterface.Nodes.Layout.Container.ContainerUIControl;
using TextBlockUIControl = UniScan.UserInterface.Nodes.Text.TextBlockUIControl;

namespace UniScan.Server.Core.UserInterface.Providers;

public class DefaultScanChannelNodeProvider : IUINodeProvider<ScanChannelLayoutContext>
{
    public int Priority => 0;
    
    public bool CanProvide(ScanChannelLayoutContext o) => true;

    public IUINode Provide(ScanChannelLayoutContext o)
    {
        return
            new ContainerUIControl(
                                   new TextBlockUIControl(new
                                                              BoundTextComponent(new AbsoluteTextComponent("{0}"),
                                                                       new Identifier("UniScan", "device",
                                                                                    "state",
                                                                                    "property", "channel",
                                                                                    "name")
                                                                          .AsBindableIdentifier(""))),
                                   new TextBlockUIControl(new
                                                              BoundTextComponent(new AbsoluteTextComponent("{0:choose(True|False):(Talking)|}"),
                                                                       new Identifier("UniScan", "device",
                                                                                    "state",
                                                                                    "property", "talking")
                                                                          .AsBindableIdentifier("false")))
                                  )
            {
                ItemSpacing = 10,
                FlowDirection = FlowDirection.LeftToRight,
            };
    }
}