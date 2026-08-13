using System.Text.Json;
using Serilog;
using Shiki.Common.Serialization.Polymorphism;
using Shiki.Common.Serialization.Polymorphism.Source.Sources;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using UniScan.Core.Serialization;
using UniScan.Network;
using UniScan.Network.Registry.Source.Sources;
using UniScan.Network.Server;
using UniScan.Server.Authentication.Session;
using UniScan.Server.Core;
using UniScan.Server.Core.Module;
using UniScan.Server.Core.Module.Modules.Internal;

namespace UniScan.Server.Host;

class UniScanHost
{
    private CancellationTokenSource _cts = new();
    private UniScanServer _server;

    private string _rootDirectory;
    
    private readonly JsonSerializerOptions _jsonOptions;
    
    private readonly ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs> _moduleStorage;
    private readonly ScannerMeta _scannerMeta;
    private readonly PacketRegistry _packetRegistry;

    public UniScanHost(string rootDirectory)
    {
        _rootDirectory = rootDirectory;
        
        _moduleStorage = new ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs>()
           .WithModulesFrom(new TypeListModuleSource(typeof(InternalUniScanServerModule)), new UniScanServerModuleInitializationArgs())
           .WithModulesFrom(new AssembliesModuleSource("modules"), new UniScanServerModuleInitializationArgs());
        
        //todo channel init????
        
        _jsonOptions = PolymorphicJsonOptionsFactory.Get();

        _packetRegistry = new PacketRegistry();
        
        _server = new UniScanServer(new SessionManager(), initializer => new ServerSocket(initializer, 9000), _packetRegistry, _moduleStorage);
        _scannerMeta = new ScannerMeta(_rootDirectory, _jsonOptions);

    }
    
    public async Task StartAsync()
    {
        await this._scannerMeta.WriteSchemaAsync();
        
        foreach (var scanner in await this._scannerMeta.LoadDtosAsync())
        {
            _server.ScannerManager.AddScanner(scanner.Key, scanner.Value.DisplayName, scanner.Value.Scanner);
        }

        //start scanners
        await this._server.ScannerManager.StartAllAsync();
        
        //start server
        await this._server.RunAsync();
    }

    public async Task Stop()
    {
        Log.Logger.Information("Stopping server...");
        
        _cts.Cancel();
        await this._server.ExitAsync();
    }

    private static async Task Main(string[] args)
    {
        // TODO System.CommandLine
        Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console(outputTemplate: UniScan.Core.Constants.ConsoleOutputTemplate)
                    .WriteTo
                    .File(Path.Combine("logs", "server", $"UniScan.Server-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log"),
                          retainedFileCountLimit: null,
                          outputTemplate: UniScan.Core.Constants.FileOutputTemplate)
                    .MinimumLevel.Debug()
                    .CreateLogger()
                    .ForContext("SourceContext", typeof(UniScanServer).Namespace);
        UniScanHost host = new(Environment.CurrentDirectory);
        
        AppDomain.CurrentDomain.ProcessExit += async (_, _) =>
        {
            host.Stop();
        };
        
        Console.CancelKeyPress += async (_, e) =>
        {
            e.Cancel = true;
            host.Stop();
        };
        
        await host.StartAsync();
    }
}