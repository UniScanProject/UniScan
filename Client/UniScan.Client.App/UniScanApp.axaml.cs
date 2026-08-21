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
using UniScan.Client.App.Module;
using UniScan.Client.App.Module.Modules.Internal;
using UniScan.Client.App.Pipeline;
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
    public static readonly SemVersion PlatformVersion = SemVersion.FromVersion(Assembly.GetEntryAssembly()!.Version);

    public static readonly ClientSoftwareInfo SoftwareInfo = new(
                                                                 Identifier,
                                                                 SemVersion.FromVersion(typeof(UniScanApp).Assembly
                                                                    .Version),
                                                                 Network.Constants.ProtocolVersion,
                                                                 "UniScan Client",
                                                                 "https://github.com/UniScanProject/UniScan"
                                                                );

    public ModuleStorage<IUniScanClientAppModule, UniScanClientAppModuleInitializationArgs>? ModuleStorage
    {
        get;
        private set;
    }
    
    public UniScanAppLoadPipeline LoadPipeline { get; private set; }
    
    public override void Initialize()
    {
        LoadPipeline = new UniScanAppLoadPipeline(this);
        AvaloniaXamlLoader.Load(this);

        LoadingComplete += OnLoadingComplete;
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        Log.Information("Loading root view");

        RootViewModel = new RootViewModel(new LoadingViewModel(this));
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
                await LoadPipeline.RunAsync();
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
    
    #region Initialization Pipeline
    internal async Task InitializeEnvironment(UniScanAppLoadPipeline.LoadContexts.Early context)
    {
        context.Status.Value = "Initializing environment";

        if (InitializePlatform == null)
            throw new ArgumentException("No platform initializer was provided");

        _hostEnvironment = await InitializePlatform();

        if (_hostEnvironment == null)
            throw new ArgumentException("Platform not initialized");

        await _hostEnvironment.StandardPaths.CreateAllAsync(_hostEnvironment.DirectoryManager);
        Log.Logger = _hostEnvironment.SerilogInitializer.GetConfiguration(_hostEnvironment).CreateLogger()
                                     .ForContext<UniScanApp>();

        _hostEnvironment.AddToDi(context.ServiceCollection);
        Log.Logger.Debug("Initialized Environment {Env}", _hostEnvironment);
    }

    internal async Task InitializeSoftwareInfo(UniScanAppLoadPipeline.LoadContexts.Early ctx)
    {
        ctx.Status.Value = "Initializing SoftwareInfo";

        ctx.ServiceCollection.AddSingleton(SoftwareInfo);
        Log.Information("{Info}", SoftwareInfo);
    }

    internal async Task InitializeModules(UniScanAppLoadPipeline.LoadContexts.Early ctx)
    {
        ctx.Status.Value = "Initializing modules";

        ModuleStorage = new ModuleStorage<IUniScanClientAppModule, UniScanClientAppModuleInitializationArgs>()
           .WithModulesFrom(new TypeListModuleSource(typeof(InternalUniScanClientAppModule)),
                            new UniScanClientAppModuleInitializationArgs(_hostEnvironment));

        string moduleDir = Path.Combine(_hostEnvironment.StandardPaths.DataPath, "modules");
        if (!(await _hostEnvironment.DirectoryManager.ExistsAsync(moduleDir)))
        {
            Log.Information("Creating new modules folder");
            await _hostEnvironment.DirectoryManager.CreateDirectoryAsync(moduleDir);
        }

        try
        {
            ModuleStorage.LoadFrom(new AssembliesModuleSource(moduleDir),
                                   new UniScanClientAppModuleInitializationArgs(_hostEnvironment));
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            Log.Error(ex, "Failed to load assembly modules");
        }

        foreach (IUniScanClientAppModule module in ModuleStorage.Modules)
        {
            module.ConfigureDi(ctx.ServiceCollection);
        }
    }

    internal async Task InitializeClient(UniScanAppLoadPipeline.LoadContexts.PreClient ctx)
    {
        ctx.Status.Value = "Initializing client and loading remotes";

        ctx.Client = await UniScanClient.CreateInstanceAsync(_hostEnvironment, SoftwareInfo);
        ctx.ServiceCollection.AddSingleton(ctx.Client);
        ctx.ServiceCollection.AddSingleton<IRemoteFactory>(_ => ctx.Client.ServiceProvider.GetRequiredService<IRemoteFactory>());
    }

    internal async Task InitializeUI(UniScanAppLoadPipeline.LoadContexts.PostClient ctx)
    {
        ctx.Status.Value = "Initializing UI";

        Log.Information("Initializing UI");

        ctx.ServiceCollection.AddSingleton<MainView>();
        ctx.ServiceCollection.AddSingleton<MainViewModel>();

        ctx.ServiceCollection.AddSingleton<ClientSettingsViewModel>();
    }

    internal async Task InitializeServiceProvider(UniScanAppLoadPipeline.LoadContexts.PreServiceProvider ctx)
    {
        ctx.Status.Value = "Building ServiceProvider";

        ctx.Services = ctx.ServiceCollection.BuildServiceProvider();
    }

    internal async Task InitializeRemotes(UniScanAppLoadPipeline.LoadContexts.PostServiceProvider ctx)
    {
        ctx.Status.Value = "Saving remotes";

        await ctx.Client!.RemoteManagerFile.SaveAsync(ctx.Client.RemoteManager);
    }

    internal async Task FinishInitialization(UniScanAppLoadPipeline.LoadContexts.PostServiceProvider ctx)
    {
        ctx.Status.Value = "Finishing up";

        ServiceProvider = ctx.Services!;
        LoadingComplete?.Invoke();
    }
#endregion
}