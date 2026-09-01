using System.Runtime.InteropServices.JavaScript;

namespace UniScan.Client.App.Platform.Browser.Interop;

public static partial class JSMethods
{
    public static async Task Initialize()
    {
        await JSHost.ImportAsync("catch", "../js/catch.js");
    }
    
    [JSImport("onExit", "catch")]
    public static partial void OnExit(int code, string? reason = null);
}