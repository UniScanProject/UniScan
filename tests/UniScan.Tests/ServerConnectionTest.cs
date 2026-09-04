using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using UniScan.Network;
using UniScan.Network.Client;
using UniScan.Network.Client.Remote.Connection.Methods;
using UniScan.Network.Protocol.Packets.Serverbound;
using UniScan.Network.Registry;
using UniScan.Network.Registry.Source.Sources;
using UniScan.Network.Server;
using UniScan.Server.Core;
using UniScan.Server.Core.Module;
using UniScan.Server.Core.Module.Modules.Internal;

namespace UniScan.Tests;

public class ServerConnectionTest
{
    [Test, Explicit]
    public async Task Test()
    {
        Log.Logger = new LoggerConfiguration()
                    .WriteTo.NUnitOutput(outputTemplate: Core.Constants.ConsoleOutputTemplate)
                    .WriteTo
                    .File(Path.Combine("logs", "tests", $"UniScan.Tests-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log"),
                          retainedFileCountLimit: null,
                          outputTemplate: Core.Constants.FileOutputTemplate)
                    .MinimumLevel.Debug()
                    .CreateLogger()
                    .ForContext("SourceContext", typeof(UniScanServer).Namespace);
        
        // UniScanServer server = new(new SessionManager(), initializer => new ServerSocket(initializer, 9000), PacketRegistry.Instance, new ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs>()
        //                               .WithModulesFrom(new InternalModuleSource(typeof(InternalUniScanServerModule)), new UniScanServerModuleInitializationArgs()));
        //
        // Log.Information("Starting server...");
        // TaskCompletionSource<bool> tcs = new();
        // Task serverTask = Task.Run(() =>
        // {
        //     tcs.SetResult(true);
        //     return server.RunAsync();
        // });
        //
        // await tcs.Task;
        // await Task.Delay(3000);
        // Log.Information("Started");

        ServiceCollection services = new();
        var sv = services.BuildServiceProvider();

        PacketRegistry reg = new();
        reg.RegisterFromSource<AssembliesPacketSource>();
        
        ClientSocket client = new(new UniScanClientChannelInitializer(reg, [], sv),
                                  new TCPRemoteConnectionMethod(new IPEndPoint(IPAddress.Any, 9000)));
        await client.StartAsync();

        await client.SendPacketAsync(GetDeviceListPacket.CreateRequest());
        
        await Task.Delay(Timeout.Infinite);
    }
}