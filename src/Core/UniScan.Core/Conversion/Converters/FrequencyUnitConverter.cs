using UniScan.Core.State.Types;

namespace UniScan.Core.Conversion.Converters;

public class FrequencyUnitConverter : ICanConvertFrom<string, FrequencyUnit>, ICanConvertTo<FrequencyUnit, string>
{
    public static FrequencyUnit ConvertFrom(string from) =>
        from.ToUpper().Replace(" ", "") switch
        {
            { } freq when freq.EndsWith("THZ") => FrequencyUnit.THz,
            { } freq when freq.EndsWith("GHZ") => FrequencyUnit.GHz,
            { } freq when freq.EndsWith("MHZ") => FrequencyUnit.MHz,
            { } freq when freq.EndsWith("KHZ") => FrequencyUnit.KHz,
            { } freq when freq.EndsWith("HZ")  => FrequencyUnit.Hz,
            _                                  => throw new ArgumentException("Invalid frequency unit")
        };

    public static FrequencyUnit ConvertFrom(string frequency, out string rest)
    {
        string freq = frequency.ToUpper().Replace(" ", "");

        switch (freq)
        {
            case not null when freq.EndsWith("THZ"):
                rest = frequency[..^"THz".Length];
                return FrequencyUnit.THz;
            case not null when freq.EndsWith("GHZ"):
                rest = frequency[..^"GHz".Length];
                return FrequencyUnit.GHz;
            case not null when freq.EndsWith("MHZ"):
                rest = frequency[..^"MHz".Length];
                return FrequencyUnit.MHz;
            case not null when freq.EndsWith("KHZ"):
                rest = frequency[..^"KHz".Length];
                return FrequencyUnit.KHz;
            case not null when freq.EndsWith("HZ"):
                rest = frequency[..^"Hz".Length];
                return FrequencyUnit.Hz;
        }

        throw new ArgumentException("Invalid frequency unit");
    }

    public static string ConvertTo(FrequencyUnit self) => self switch
    {
        FrequencyUnit.THz => "THz",
        FrequencyUnit.GHz => "GHz",
        FrequencyUnit.MHz => "MHz",
        FrequencyUnit.KHz => "KHz",
        FrequencyUnit.Hz  => "Hz",
        _                  => throw new ArgumentException("Invalid frequency unit")
    };
}