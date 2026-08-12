using UniScan.Core.State.Types;

namespace UniScan.Core.Conversion.Converters;

using HzType = ulong;

public class FrequencyConverter : ICanConvertFrom<string, HzType>, ICanConvertTo<HzType, string>
{
    public static HzType ConvertFrom(string from)
    {
        if (string.IsNullOrWhiteSpace(from)) return 0;

        FrequencyUnit unit = FrequencyUnitConverter.ConvertFrom(from, out string num);

        return ConvertFrom(num, unit);
    }

    public static HzType ConvertFrom(string from, FrequencyUnit unit)
    {
        if (string.IsNullOrWhiteSpace(from)) return 0;

        if (double.TryParse(from, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double frequency))
        {

            return (HzType)(frequency * (HzType)unit);
        }

        throw new ArgumentException("Invalid frequency string");
    }

    public static string ConvertTo(HzType self) => throw new NotImplementedException();
}