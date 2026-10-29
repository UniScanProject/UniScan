using Shiki.Common.Identity;

namespace UniScan.Client.App.Core;

public static class Constants
{
    public static class Settings
    {
        public static readonly Identifier MainCategory = UniScanApp.Identifier.Derived("settings", "category", "main");
        public static readonly Identifier MainStorage = UniScanApp.Identifier.Derived("settings", "storage", "central");
    }
}