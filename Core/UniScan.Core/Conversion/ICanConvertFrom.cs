namespace UniScan.Core.Conversion;

/// <summary>
/// Interface which defines a static converter function from TFrom to TSelf.
/// </summary>
/// <typeparam name="TFrom">The type to convert from</typeparam>
/// <typeparam name="TSelf">The type to convert to (usually self)</typeparam>
public interface ICanConvertFrom<in TFrom, out TSelf>
{
    /// <summary>
    /// Converts a value of type TFrom to TSelf.
    /// </summary>
    /// <param name="from">The value to convert from</param>
    /// <returns>The converted value</returns>
    public static abstract TSelf ConvertFrom(TFrom from);
}