using System.Drawing;
using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Identity;
using Shiki.Internationalization;
using Shiki.Internationalization.Component;
using UniScan.Network;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;
using UniScan.UserInterface.Properties.Styling;
using ContainerUIControl = UniScan.UserInterface.Nodes.Layout.Container.ContainerUIControl;
using TextBlockUIControl = UniScan.UserInterface.Nodes.Text.TextBlockUIControl;

namespace UniScan.Server.Core.UserInterface.Providers;

public class DefaultDeviceNodeProvider(IServiceProvider provider) : IUINodeProvider<DeviceLayoutContext>
{
    private IUISSRManager _manager;
    
    public int Priority => 0;

    public bool CanProvide(DeviceLayoutContext o) => true;

    public IUINode Provide(DeviceLayoutContext o)
    {
        if (_manager == null)
            _manager = provider.GetRequiredService<IUISSRManager>();
        
        return new ContainerUIControl(
                                      new ContainerUIControl(
                                                             new TextBlockUIControl(new
                                                                 BoundTextComponent(new TranslatableTextComponent("UniScan:i18n/ssr/property/squelch"),
                                                                     new Identifier("UniScan", "device",
                                                                             "state",
                                                                             "property", "squelch")
                                                                        .AsBindableIdentifier())),
                                                             new TextBlockUIControl(new
                                                                 BoundTextComponent(new TranslatableTextComponent("UniScan:i18n/ssr/property/volume"),
                                                                     new Identifier("UniScan", "device",
                                                                             "state", "property",
                                                                             "volume")
                                                                        .AsBindableIdentifier()))
                                                            )
                                      {
                                          ItemSpacing = 10,
                                          FlowDirection = FlowDirection.RightToLeft,
                                      },
                                      _manager.ResolveNode(new ScanZoneLayoutContext(o.Host, o.State)) ?? new TextBlockUIControl(new AbsoluteTextComponent("Failed to find node provider for zone display")),
                                      _manager.ResolveNode(new ScanListLayoutContext(o.Host, o.State)) ?? new TextBlockUIControl(new AbsoluteTextComponent("Failed to find node provider for list display")),
                                      _manager.ResolveNode(new ScanGroupLayoutContext(o.Host, o.State)) ?? new TextBlockUIControl(new AbsoluteTextComponent("Failed to find node provider for group display")),
                                      _manager.ResolveNode(new ScanChannelLayoutContext(o.Host, o.State)) ?? new TextBlockUIControl(new AbsoluteTextComponent("Failed to find node provider for channel display"))
                                     )
        {
            ItemSpacing = 20,
            Style =
            {
                CornerRadius = new Vector4(10),
                Padding = new Vector4(10),
                Margin = new Vector4(10),
                BackgroundColor = Color.DimGray,
                BoundConstraints =
                {
                    MaxBounds = new Vector2(float.NaN, 300)
                }
            }
        };
    }
}