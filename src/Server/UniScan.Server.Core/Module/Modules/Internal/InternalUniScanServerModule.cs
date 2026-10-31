using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Semver;
using Shiki.Common.Extensions;
using Shiki.Common.Identity;
using Shiki.Common.Util;
using Shiki.FileSystem.Storage;
using Shiki.FileSystem.Storage.Serializer;
using Shiki.Internationalization.Component;
using Shiki.ModuleManagement.Module;
using UniScan.Core.Serialization;
using Shiki.Settings;
using Shiki.Settings.Category;
using Shiki.Settings.Option.Storage;
using UniScan.Network.Socket.Configuration;
using UniScan.Server.Core.Module.Modules.Internal.Filter;
using UniScan.Server.Core.Module.Modules.Internal.Handler;
using UniScan.Server.Core.UserInterface;
using UniScan.Server.Core.UserInterface.Providers;

namespace UniScan.Server.Core.Module.Modules.Internal;

public class InternalUniScanServerModule : IUniScanServerModule, IInitializableModule<UniScanServerModuleInitializationArgs>
{
    /// <inheritdoc/>
    public Identifier Id => Identifier.WithNamespaceOfType<UniScanServer>("modules", "internal");
    
    private UniScanServer _server = null!;

    public void OnInitialize(UniScanServerModuleInitializationArgs args)
    {
        _server = args.ServerInstance;
        
        FileStorage<MainOptionGroup> storage = new FileStorage<MainOptionGroup>(Path.Combine(args.ServerInstance.RootDirectory, "settings"), "settings.json",
                                                                          args.DirectoryManager, args.FileManager,
                                                                          new JsonStorageSerializer<MainOptionGroup>(PolymorphicJsonTypeInfoResolverFactory.FancySerializerOptions));
        var optionStorage = new OptionStorage<MainOptionGroup, FileStorage<MainOptionGroup>>(storage);
        
        args.SettingsBuilder.AddStorage(Constants.Settings.MainStorage, optionStorage)
            .AddCategory(new SettingsCategory(Constants.Settings.MainCategory, new AbsoluteTextComponent("Main")))
            .AddGroup(UniScanServer.Identifier.Derived("settings", "group", "main"), new MainOptionGroup(), Constants.Settings.MainStorage);
    }

    public void ConfigureDi(IServiceCollection services)
    {
        services.AddTransient<IUISSRContextSessionFactory, DeviceSSRContextSessionFactory>();
        
        services.AddSingleton<IUINodeProvider, DefaultDeviceNodeProvider>();
        services.AddSingleton<IUINodeProvider, DefaultScanChannelNodeProvider>();
        services.AddSingleton<IUINodeProvider, DefaultScanGroupNodeProvider>();
        services.AddSingleton<IUINodeProvider, DefaultScanListNodeProvider>();
        services.AddSingleton<IUINodeProvider, DefaultScanZoneNodeProvider>();
        
        services.AddTransient<UIContextUnsubscribePacketHandler>();
        services.AddTransient<DeviceUnsubscribePacketHandler>();
        services.AddTransient<UIContextSubscribePacketHandler>();
        services.AddTransient<SubscribePacketHandler>();
        services.AddTransient<GetDeviceListPacketHandler>();
        services.AddTransient<ClientSoftwareInfoPacketHandler>();
        services.AddTransient<DisconnectPacketHandler>();

        services.AddSingleton(new AcceptedClientPacketFilter());
        
        services.AddSingleton<IPipelineConfigurator, InternalUniScanServerPipelineConfigurator>();
    }

    public void Dispose()
    {
    }
}