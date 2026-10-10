using Avalonia.Media;
using Color = System.Drawing.Color;

namespace UniScan.Client.App.Core.Extensions;

public static class ColorExtensions
{
    extension(Color color)
    {
        public Avalonia.Media.Color AsAvaloniaColor() => new(color.A, color.R, color.G, color.B);
        public Brush AsBrush() => new SolidColorBrush(color.AsAvaloniaColor());
    }
}