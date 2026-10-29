using System.Collections.Generic;
using System.Text.Json.Serialization;
using Shiki.Common.Identity;
using Shiki.Settings.Option;
using UniScan.Network.Group;

namespace UniScan.Client.App.Core.Module.Modules.Internal.Options;

public class NetworkOptionGroup : IIdentifiableOptionGroup
{
    public static Identifier Identifier { get; } = UniScanApp.Identifier.Derived("settings", "group", "network");
    public static Identifier AdvancedNetworkOptionsCategoryIdentifier { get; } = UniScanApp.Identifier.Derived("settings", "category", "advanced", "network");
    public static Identifier NetworkOptionsCategoryIdentifier { get; } = UniScanApp.Identifier.Derived("settings", "category", "network");
    public static Identifier StorageIdentifier { get; } = UniScanApp.Identifier.Derived("settings", "storage", "network");
    
    [JsonIgnore]
    public SettingOption<string> NetworkGroupMethodOption { get; } = new(UniScanApp.Identifier.Derived("settings",
                                                                             "option",
                                                                             "network_group_method"),
                                                                         AdvancedNetworkOptionsCategoryIdentifier,
                                                                         MultithreadedNetworkGroupManager.Id);
  
    
    [JsonPropertyName("network_group_method")]
    public string NetworkGroupMethod
    {
        get => NetworkGroupMethodOption.Value;
        set => NetworkGroupMethodOption.SetValue(value);
    }

    public IEnumerable<IOption> GetOptions()
    {
        yield return NetworkGroupMethodOption;
    }
}