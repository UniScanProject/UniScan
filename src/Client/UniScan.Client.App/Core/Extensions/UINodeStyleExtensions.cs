using Avalonia.Controls;
using Avalonia.Media;
using UniScan.UserInterface;
using UniScan.UserInterface.Properties.Styling;

namespace UniScan.Client.App.Core.Extensions;

public static class UINodeStyleExtensions
{
    extension(UINodeStyle style)
    {
        public Border BuildStyledBorder() => new()
        {
            Width = style.Bounds.X,
            Height = style.Bounds.Y,
            
            MinWidth = !float.IsNaN(style.BoundConstraints.MinBounds.X) ? style.BoundConstraints.MinBounds.X : 0.0,
            MaxWidth = !float.IsNaN(style.BoundConstraints.MaxBounds.X) ? style.BoundConstraints.MaxBounds.X : double.PositiveInfinity,
            MinHeight = !float.IsNaN(style.BoundConstraints.MinBounds.Y) ? style.BoundConstraints.MinBounds.Y : 0.0,
            MaxHeight = !float.IsNaN(style.BoundConstraints.MaxBounds.Y) ? style.BoundConstraints.MaxBounds.Y : double.PositiveInfinity,
            
            Margin = style.Margin.AsThickness(), //TODO I think some of these do not treat NaN the same
            Padding = style.Padding.AsThickness(),
            CornerRadius = style.CornerRadius.AsCornerRadius(),
            Background = style.BackgroundColor.AsBrush(),
            BorderBrush = style.Border.Color.AsBrush(),
            BorderThickness = style.Border.Thickness.AsThickness(),
            HorizontalAlignment = style.Position.HorizontalPosition.AsHorizontalAlignment(),
            VerticalAlignment = style.Position.VerticalPosition.AsVerticalAlignment(),
        };
    }
}