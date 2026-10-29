using System;
using System.Diagnostics.CodeAnalysis;
using UniScan.Client.App.UI.Validation;

namespace UniScan.Client.App.Core;

public abstract class EndpointValidatorParser : IValidatorParser
{
    public static bool Validate([NotNullWhen(true)] string? s, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(s) || !Uri.TryCreate("shit://" + s, UriKind.Absolute, out Uri? uri))
            return false;

        if (uri.Port == 0 || string.IsNullOrWhiteSpace(uri.Host))
            return false;
        
        return uri.HostNameType is UriHostNameType.Dns or UriHostNameType.IPv4 or UriHostNameType.IPv6;
    }
}