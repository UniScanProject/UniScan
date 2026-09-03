using Avalonia.Layout;
using UniScan.UserInterface.Definitions;

namespace UniScan.Client.App.Core.Extensions;

public static class PositionExtensions
{
    extension(HorizontalPosition pos)
    {
        public HorizontalAlignment AsHorizontalAlignment() => pos switch
        {
            HorizontalPosition.Left   => HorizontalAlignment.Left,
            HorizontalPosition.Center => HorizontalAlignment.Center,
            HorizontalPosition.Right => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Stretch
        };
    }
    
    extension(VerticalPosition pos)
    {
        public VerticalAlignment AsVerticalAlignment() => pos switch
        {
            VerticalPosition.Top    => VerticalAlignment.Top,
            VerticalPosition.Center => VerticalAlignment.Center,
            VerticalPosition.Bottom => VerticalAlignment.Bottom,
            _                       => VerticalAlignment.Stretch
        };
    }
}