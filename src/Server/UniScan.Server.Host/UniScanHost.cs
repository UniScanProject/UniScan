using System.Text.Json;
using System.Text.Json.Schema;
using System.Text.Json.Serialization.Metadata;
using Serilog;
using Shiki.Common.Extensions;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Serialization.Polymorphism;
using Shiki.Common.Serialization.Polymorphism.Source.Sources;
using Shiki.FileSystem.Implementations.Native;
using Shiki.FileSystem.Storage.Directory.KeyValue;
using Shiki.FileSystem.Storage.Serializer;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using UniScan.Core.Serialization;
using Shiki.Settings;
using UniScan.Network;
using UniScan.Network.Registry;
using UniScan.Network.Registry.Source.Sources;
using UniScan.Network.Server;
using UniScan.Server.Authentication.Session;
using UniScan.Server.Core;
using UniScan.Server.Core.Host;
using UniScan.Server.Core.Module;
using UniScan.Server.Core.Module.Modules.Internal;
using UniScan.Server.Core.Pipeline;
using UniScan.Server.Core.Social;
using UniScan.Server.Host.Pipeline;

namespace UniScan.Server.Host;

class UniScanHost
{
    private CancellationTokenSource _cts = new();
    private UniScanServer _server;
    private string _rootDirectory;
    
    private bool _stopping = false;
    private readonly SemaphoreSlim _stopSemaphore = new(1, 1);

    
    public UniScanHost(string rootDirectory)
    {
        _rootDirectory = rootDirectory;
        _server = new UniScanServer(rootDirectory);
    }
    
    public async Task StartAsync()
    {
        UniScanServerHostInitializationPipeline hostPipeline = new();
        UniScanServerHostInitializationPipeline.TaskContexts.HostReady hostContext = await hostPipeline.RunAsync(_server, _rootDirectory);

        UniScanServerInitializationPipeline serverPipeline = new(_server);
        await serverPipeline.RunAsync(new UniScanServerInitializationPipeline.TaskContexts.PreEnvironment(_server,
                                  hostContext.RootDirectory, hostContext.FileManager, hostContext.DirectoryManager, hostContext.ModuleStorage,
                                  hostContext.SettingsBuilder));
        
        await _server.RemoteInfoManager.LoadAsync();
        
        await File.WriteAllTextAsync(Path.Combine(_rootDirectory, "devices", "device.schema.json"), JsonSchemaUtil.GetSchema<DeviceHostDto>(PolymorphicJsonTypeInfoResolverFactory.FancySerializerOptions));

        var storage = new DirectoryKeyValueStorage<DeviceHostDto>(
                                                                  Path.Combine(_rootDirectory, "devices"),
                                                                  hostContext.DirectoryManager,
                                                                  hostContext.FileManager,
                                                                  new JsonStorageSerializer<DeviceHostDto>(PolymorphicJsonTypeInfoResolverFactory.FancySerializerOptions));
        
        var loaded = await storage.LoadAsync();
        foreach (var err in loaded.Errors)
        {
            Log.Warning(err.Exception, "Failed to load device file '{Path}'", err.Item);
        }
        
        foreach (var device in loaded.Items)
        {
            _server.DeviceManager.AddDevice(device.Key.ToSlug<SnakeSlugFormatter>(), device.Value.DisplayName, device.Value.Device);
        }
        
        //start server
        await _server.RunAsync(_cts.Token);
    }

    public async Task Stop()
    {
        await _stopSemaphore.WaitAsync();

        try
        {
            if (_stopping)
                return;

            _stopping = true;

            Log.Information("Stopping server...");

            _cts.Cancel();
            
            Log.Information("Exiting...");
            await _server.ExitAsync();
        }
        finally
        {
            _stopSemaphore.Release();
        }
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
        
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            host.Stop().GetAwaiter().GetResult();
        };
        
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            host.Stop().GetAwaiter().GetResult();
        };
        
        await host.StartAsync();
    }
}