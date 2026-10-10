using Avalonia.Layout;
using UniScan.UserInterface.Properties.Styling;

namespace UniScan.Client.App.Core.Extensions;

public static class FlowDirectionExtensions
{
    extension(FlowDirection flowDirection)
    {
        public Orientation AsAvaloniaOrientation() => flowDirection switch
        {
            FlowDirection.TopToBottom => Orientation.Vertical,
            FlowDirection.BottomToTop => Orientation.Vertical,
            FlowDirection.LeftToRight => Orientation.Horizontal,
            FlowDirection.RightToLeft => Orientation.Horizontal,
            _                                                   => Orientation.Horizontal
        };

        public Avalonia.Media.FlowDirection AsAvaloniaFlowDirection() => flowDirection switch
        {
            FlowDirection.LeftToRight => Avalonia.Media.FlowDirection.LeftToRight,
            FlowDirection.RightToLeft => Avalonia.Media.FlowDirection.RightToLeft,
            _                                                   => Avalonia.Media.FlowDirection.LeftToRight
        };
    }
}