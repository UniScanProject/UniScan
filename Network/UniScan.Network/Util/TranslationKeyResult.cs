using Shiki.Common.Identity;
using Shiki.Common.Util;

namespace UniScan.Network.Util;

/// <summary>
/// Holds a translation key and whether the result was a success
/// </summary>
/// <param name="success">Whether the result was successful</param>
/// <param name="info">The info translation key</param>
public class TranslationKeyResult(bool success, Identifier? info)
{
    /// <summary>
    /// The info translation key
    /// </summary>
    public Identifier? Info { get; } = info;

    /// <summary>
    /// Whether the result is a success
    /// </summary>
    public bool Success { get; } = success;
}