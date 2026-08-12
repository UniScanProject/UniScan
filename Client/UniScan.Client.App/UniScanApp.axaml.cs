using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Semver;
using Serilog;
using Shiki.Common.Extensions;
using Shiki.Common.Identity;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using UniScan.Client.App.Module;
using UniScan.Client.App.Module.Modules.Internal;
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
    
    public RootViewModel RootViewModel { get; private set;  }

    public static Func<Task<HostEnvironment>>? InitializePlatform;
    private event Func<Task>? LoadingComplete;

    private HostEnvironment _hostEnvironment = null!;
    
    public static readonly Identifier Identifier = UniScanClient.ClientIdentifier.Derived("app");
    
    public static readonly ClientSoftwareInfo SoftwareInfo = new(
                                                                 Identifier,
                                                                 SemVersion.FromVersion(Assembly.GetCallingAssembly().Version),
                                                                 1,
                                                                 "UniScan Client",
                                                                 "https://github.com/UniScanProject/UniScan"
                                                                );
    
    public ModuleStorage<IUniScanClientAppModule, UniScanClientAppModuleInitializationArgs>? ModuleStorage { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        LoadingComplete += OnLoadingComplete;
    }

    /// <summary>
    /// Initializes objects on background thread created during OnFrameworkInitializationCompleted
    ///
    /// Has to be done to allow for async, once its done we switch away from the loading screen view.
    /// </summary>
    private async Task InitializeAsync()
    {
        if (InitializePlatform == null)
            throw new ArgumentException("No platform initializer was provided");
        
        _hostEnvironment = await InitializePlatform();
     
        if (_hostEnvironment == null)
            throw new ArgumentException("Platform not initialized");
        
        ServiceCollection services = new();

        services.AddSingleton(SoftwareInfo);

        await _hostEnvironment.StandardPaths.CreateAllAsync(_hostEnvironment.DirectoryManager);
        Log.Logger = _hostEnvironment.SerilogInitializer.GetConfiguration(_hostEnvironment).CreateLogger().ForContext<UniScanApp>();
        
        Log.Information("{Info}", SoftwareInfo);
        
        _hostEnvironment.AddToDi(services);
        Log.Logger.Debug("Initialized Environment {Env}", _hostEnvironment);
        
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
            module.ConfigureDi(services);
        }

        UniScanClient client = await UniScanClient.CreateInstanceAsync(_hostEnvironment, SoftwareInfo);
        services.AddSingleton(client);
        services.AddSingleton<IRemoteFactory>(_ => client.ServiceProvider.GetRequiredService<IRemoteFactory>());
        
        Log.Information("Initializing UI");
        
        services.AddSingleton<MainView>();
        services.AddSingleton<MainViewModel>();
        
        services.AddSingleton<ClientSettingsViewModel>();
        
        ServiceProvider = services.BuildServiceProvider();

        await using Stream stream =
            await _hostEnvironment.FileManager.GetStreamAsync("test.shit", FileMode.Create, FileAccess.Write,
                                                              FileShare.None);


        var garbage = new
        {
            Hello = "World"
        };
        try
        {
            await JsonSerializer.SerializeAsync(stream, garbage);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "dhfjshfjkd");
        }

        await client.RemoteManagerFile.SaveAsync(client.RemoteManager);
        LoadingComplete?.Invoke();
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        Log.Information("Loading root view");
        
        RootViewModel = new RootViewModel(new LoadingViewModel());
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

        Task.Run(InitializeAsync);
        
        base.OnFrameworkInitializationCompleted();
    }

    private async Task OnLoadingComplete()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            RootViewModel.CurrentSubpage = ServiceProvider.GetRequiredService<MainViewModel>();
        });
    }
}