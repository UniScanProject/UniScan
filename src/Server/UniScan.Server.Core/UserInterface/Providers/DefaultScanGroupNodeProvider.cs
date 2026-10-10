using Shiki.Common.Identity;
using Shiki.Internationalization.Component;
using UniScan.Network;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;
using TextBlockUIControl = UniScan.UserInterface.Nodes.Text.TextBlockUIControl;

namespace UniScan.Server.Core.UserInterface.Providers;

public class DefaultScanGroupNodeProvider : IUINodeProvider<ScanGroupLayoutContext>
{
    public int Priority => 0;
    
    public bool CanProvide(ScanGroupLayoutContext o) => true;

    public IUINode Provide(ScanGroupLayoutContext o) =>
        new TextBlockUIControl(new BoundTextComponent(new AbsoluteTextComponent("{0}"),
                                                      new Identifier("UniScan", "device", "state", "property", "group",
                                                                     "name").AsBindableIdentifier("")))
        {
            FontSize = 16
        };
}