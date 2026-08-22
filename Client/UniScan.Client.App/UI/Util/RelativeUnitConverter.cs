using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace UniScan.Client.App.UI.Util;

public class RelativeUnitConverter : MarkupExtension, IValueConverter
{
    public int? Min { get; set; }
    public int? Max { get; set; }
    public string? Percentage { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double current || double.IsNaN(current))
            return double.NaN;

        double res = current;

        if (!string.IsNullOrEmpty(Percentage))
        {
            if (double.TryParse(Percentage, NumberStyles.Any, CultureInfo.InvariantCulture, out double p))
            {
                res = current * p;
            }
        }

        if (Min != null)
            res = Math.Max(res, Min.Value);

        if (Max != null)
            res = Math.Min(res, Max.Value);

        return res;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        AvaloniaProperty.UnsetValue;

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}