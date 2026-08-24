using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using R3;
using Semver;
using Serilog;
using Shiki.Common.Extensions;
using Shiki.Common.Identity;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using UniScan.Client.App.Core.Module;
using UniScan.Client.App.Core.Pipeline.Initialization;
using UniScan.Client.App.ViewModels;
using UniScan.Client.Core;
using UniScan.Client.Core.DI.Factory;
using UniScan.Client.Core.Module.Modules.Internal;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Packet.Packets.Serverbound.Client;
using UniScan.Platform;
using UniScan.Platform.DependencyInjection;
using MainView = UniScan.Client.App.Views.MainView;
using MainViewModel = UniScan.Client.App.ViewModels.MainViewModel;
using MainWindow = UniScan.Client.App.Views.MainWindow;
using RootView = UniScan.Client.App.Views.RootView;
using RootViewModel = UniScan.Client.App.ViewModels.RootViewModel;

namespace UniScan.Client.App;

public partial class UniScanApp : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public RootViewModel RootViewModel { get; private set; }

    public static Func<Task<HostEnvironment>>? InitializePlatform;
    private event Func<Task>? LoadingComplete;

    private HostEnvironment _hostEnvironment = null!;

    public static readonly Identifier Identifier = UniScanClient.ClientIdentifier.Derived("app");
    public static readonly SemVersion PlatformVersion = SemVersion.Parse(Assembly.GetEntryAssembly()!.InformationalVersionString);

    public static readonly ClientSoftwareInfo SoftwareInfo = new(
                                                                 Identifier,
                                                                 SemVersion.Parse(typeof(UniScanApp).Assembly
                                                                    .InformationalVersionString),
                                                                 Network.Constants.ProtocolVersion,
                                                                 "UniScan Client",
                                                                 "https://github.com/UniScanProject/UniScan"
                                                                );

    public ModuleStorage<IUniScanClientAppModule, UniScanClientAppModuleInitializationArgs>? ModuleStorage
    {
        get;
        private set;
    }

    public UniScanAppInitializationPipeline InitializationPipeline { get; private set; } = null!;
    
    public override void Initialize()
    {
        InitializationPipeline = new UniScanAppInitializationPipeline(this);
        AvaloniaXamlLoader.Load(this);

        LoadingComplete += OnLoadingComplete;
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        Log.Information("Loading root view");

        RootViewModel = new RootViewModel(new LoadingViewModel("Loading...", InitializationPipeline.Pipeline));
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new MainWindow
                {
                    DataContext = RootViewModel
                };
                break;
            case IActivityApplicationLifetime singleViewFactoryApplicationLifetime:
                singleViewFactoryApplicationLifetime.MainViewFactory =
                    () => new RootView() { DataContext = RootViewModel };
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new RootView()
                {
                    DataContext = RootViewModel
                };
                break;
        }


        // Initializes objects on background thread created during OnFrameworkInitializationCompleted
        //
        // Has to be done to allow for async, once it's done we switch away from the loading screen view.
        Task.Run(async () =>
        {
            try
            {
                await InitializationPipeline.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occured during initialization");
                throw;
            }
        });

        base.OnFrameworkInitializationCompleted();
    }

    private async Task OnLoadingComplete()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            RootViewModel.CurrentSubpage =
                ServiceProvider.GetRequiredService<MainViewModel>();
        });
    }
}