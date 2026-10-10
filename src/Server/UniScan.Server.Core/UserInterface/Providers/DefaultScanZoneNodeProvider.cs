using System.Numerics;
using Shiki.Common.Identity;
using Shiki.Internationalization.Component;
using UniScan.Core.State.Radio;
using UniScan.Network;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;
using TextBlockUIControl = UniScan.UserInterface.Nodes.Text.TextBlockUIControl;

namespace UniScan.Server.Core.UserInterface.Providers;

public class DefaultScanZoneNodeProvider : IUINodeProvider<ScanZoneLayoutContext>
{
    public int Priority => 0;
    
    public bool CanProvide(ScanZoneLayoutContext o) => true;

    public IUINode Provide(ScanZoneLayoutContext o) => new TextBlockUIControl(new BoundTextComponent(new AbsoluteTextComponent("{0}"), new Identifier("UniScan", "device", "state", "property", "zone", "name").AsBindableIdentifier("")))
    {
        FontSize = 16
    };
}