using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Identity;
using Shiki.FileSystem.Storage;
using Shiki.FileSystem.Storage.Serializer;
using Shiki.Internationalization.Component;
using Shiki.ModuleManagement.Module;
using UniScan.Client.App.Core.Module.Modules.Internal.ViewConverter;
using UniScan.Client.App.UI.Conversion;
using UniScan.Client.App.UI.Conversion.Options;
using UniScan.Client.App.UI.ServersideRendering;
using UniScan.Client.Core.Module.Modules.Internal.Handler;
using Shiki.Settings;
using Shiki.Settings.Category;
using Shiki.Settings.Option;
using Shiki.Settings.Option.Storage;
using UniScan.Client.App.Core.Module.Modules.Internal.Options;
using UniScan.Core.Serialization;
using UniScan.Network.Socket.Configuration;

namespace UniScan.Client.App.Core.Module.Modules.Internal;

public class InternalUniScanClientAppModule : IUniScanClientAppModule, IInitializableModule<UniScanClientAppModuleInitializationArgs>
{
    /// <inheritdoc/>
    public Identifier Id => UniScanApp.Identifier.Derived("modules", "internal");

    
    public void OnInitialize(UniScanClientAppModuleInitializationArgs args)
    {
        FileStorage<NetworkOptionGroup> storage = new FileStorage<NetworkOptionGroup>(args.PlatformStandardPaths.ConfigPath, "network.json",
                                                                                    args.DirectoryManager, args.FileManager,
                                                                                    new JsonStorageSerializer<NetworkOptionGroup>(PolymorphicJsonTypeInfoResolverFactory.FancySerializerOptions));
        var optionStorage = new OptionStorage<NetworkOptionGroup, FileStorage<NetworkOptionGroup>>(storage);
        
        args.SettingsBuilder.AddStorage(NetworkOptionGroup.StorageIdentifier, optionStorage)
            .AddCategory(new SettingsCategory(Constants.Settings.MainCategory, new TranslatableTextComponent("UniScan:client/i18n/options/category/main")))
            .AddCategory(new SettingsCategory(NetworkOptionGroup.NetworkOptionsCategoryIdentifier, new TranslatableTextComponent("UniScan:client/i18n/options/category/network")))
            .AddCategory(new SettingsCategory(NetworkOptionGroup.AdvancedNetworkOptionsCategoryIdentifier, new TranslatableTextComponent("UniScan:client/i18n/options/category/network/advanced"))
             {
                 Parent = NetworkOptionGroup.NetworkOptionsCategoryIdentifier
             })
            .AddGroup(new NetworkOptionGroup(), NetworkOptionGroup.StorageIdentifier);
    }
    
    public void ConfigureDi(IServiceCollection services)
    {
        services.AddSingleton<IUINodeViewConverter, TextBlockUIControlViewConverter>();
        services.AddSingleton<IUINodeViewConverter, ContainerUIControlViewConverter>();
        
        services.AddSingleton<IPipelineConfigurator, InternalUniScanClientAppPipelineConfigurator>();

        services.AddSingleton<IOptionUIComponentProvider, StringOptionUIComponentProvider>();
        services.AddSingleton<IOptionUIComponentProvider, BooleanOptionUIComponentProvider>();
    }

    public void Dispose()
    {
        
    }
}