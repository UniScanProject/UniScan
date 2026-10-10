using System.Numerics;
using Shiki.Common.Identity;
using Shiki.Internationalization.Component;
using UniScan.Core.State.Radio;
using UniScan.Network;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;
using TextBlockUIControl = UniScan.UserInterface.Nodes.Text.TextBlockUIControl;

namespace UniScan.Server.Core.UserInterface.Providers;

public class DefaultScanListNodeProvider : IUINodeProvider<ScanListLayoutContext>
{
    public int Priority => 0;
    
    public bool CanProvide(ScanListLayoutContext o) => true;

    public IUINode Provide(ScanListLayoutContext o) => new TextBlockUIControl(new BoundTextComponent(new AbsoluteTextComponent("{0}"), new Identifier("UniScan", "device", "state", "property", "list", "name").AsBindableIdentifier("")))
    {
        FontSize = 14,
        Style =
        {
            Padding = new Vector4(0)
        }
    };
}