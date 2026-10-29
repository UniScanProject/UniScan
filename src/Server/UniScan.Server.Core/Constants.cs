using Shiki.Common.Identity;

namespace UniScan.Server.Core;

public class Constants
{
    public static class Settings
    {
        public static readonly Identifier MainCategory = UniScanServer.Identifier.Derived("settings", "category", "main");
        public static readonly Identifier MainStorage = UniScanServer.Identifier.Derived("settings", "storage", "central");
    }
}