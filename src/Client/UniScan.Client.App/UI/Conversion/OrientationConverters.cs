using Avalonia;
using Avalonia.Data.Converters;

namespace UniScan.Client.App.UI.Conversion;

public static class OrientationHelpers
{
    public static bool IsWidescreen(double w) => w >= 800;
    public static bool IsPortrait(double w) => !IsWidescreen(w);
}

public static class OrientationConverters
{
    public static FuncValueConverter<double, bool> IsWidescreen { get; } = new(OrientationHelpers.IsWidescreen);
    public static FuncValueConverter<double, bool> IsPortrait { get; } = new(OrientationHelpers.IsPortrait);
}