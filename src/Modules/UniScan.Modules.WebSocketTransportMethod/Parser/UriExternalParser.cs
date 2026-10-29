#if UNISCAN_APP
using System.Diagnostics.CodeAnalysis;
using UniScan.Client.App.UI.Validation;

namespace UniScan.Modules.WebSocketTransportMethod.Parser;

public abstract class UriValidatorParser : IValidatorParser
{
    public static bool Validate([NotNullWhen(true)] string? s, IFormatProvider? provider) => Uri.TryCreate(s, UriKind.Absolute, out Uri? uri) && !string.IsNullOrWhiteSpace(uri.Host) && uri.Port > 0;
}
#endif