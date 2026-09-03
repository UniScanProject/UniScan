using Avalonia.Controls;
using Avalonia.Media;
using UniScan.UserInterface;
using UniScan.UserInterface.Definitions;

namespace UniScan.Client.App.Core.Extensions;

public static class UINodeStyleExtensions
{
    extension(UINodeStyle style)
    {
        public Border BuildStyledBorder() => new Border()
        {
            Margin = style.Margin.AsThickness(),
            Padding = style.Padding.AsThickness(),
            CornerRadius = style.CornerRadius.AsCornerRadius(),
            Background = new SolidColorBrush(style.BackgroundColor),
            BorderBrush = new SolidColorBrush(style.Border.Color),
            BorderThickness = style.Border.Thickness.AsThickness(),
            HorizontalAlignment = style.Position.HorizontalPosition.AsHorizontalAlignment(),
            VerticalAlignment = style.Position.VerticalPosition.AsVerticalAlignment(),
        };
    }
}