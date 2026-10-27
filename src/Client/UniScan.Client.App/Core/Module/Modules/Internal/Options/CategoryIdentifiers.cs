using Shiki.Common.Identity;

namespace UniScan.Client.App.Core.Module.Modules.Internal.Options;

public static class CategoryIdentifiers
{
    public static Identifier AdvancedCategoryIdentifier { get; } = UniScanApp.Identifier.Derived("settings", "category", "advanced");
}