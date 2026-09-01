namespace UniScan.Core.Conversion;

/// <summary>
/// Interface which defines a static converter function from TSelf to TTo.
/// </summary>
/// <typeparam name="TSelf">The type to convert from (usually self)</typeparam>
/// <typeparam name="TTo">The type to convert to</typeparam>
public interface ICanConvertTo<in TSelf, out TTo>
{
    /// <summary>
    /// Converts a value of type TSelf to TTo.
    /// </summary>
    /// <param name="self">The value (usually self) to convert from</param>
    /// <returns>The converted value</returns>
    public static abstract TTo ConvertTo(TSelf self);
}