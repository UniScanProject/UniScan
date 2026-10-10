using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shiki.Common.Identity;
using Shiki.Internationalization.Component;
using Shiki.ModuleManagement;
using Shiki.TaskPipeline;
using Shiki.TaskPipeline.Context;
using UniScan.Client.Core.Module;
using UniScan.Client.Core.Remote.Connection;
using UniScan.Client.Core.Remote.Connection.Status;
using UniScan.Client.Core.Remote.Device;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Protocol.Packets.Serverbound;
using UniScan.Network.Protocol.Packets.Serverbound.Client;

namespace UniScan.Client.Core.Remote.Pipeline;

public partial class RemoteConnectionPipeline
{
    public async Task StartConnection(TaskContexts.ConnectionContext ctx, ITaskExecutionContext execCtx)
    {
        ctx.RemoteServerMutationProxy.SetConnectionStatus(new DefaultConnectionStatusContext(ConnectionState.Connecting));
        execCtx.Status.Value = $"Connecting to {ctx.RemoteServer.ConnectionMethod.ToDisplayString()}";
        
        await ctx.RemoteServer.Socket.StartAsync();
        if (!ctx.RemoteServer.Socket.Connected)
        {
            throw new InvalidOperationException("Connection failed!");
        }
        
        ctx.RemoteServer.Socket.Channel?.GetAttribute(ServerAttributes.ServerAttribute).Set(ctx.RemoteServer);
    }
    
    public async Task Handshake(TaskContexts.NegotiationContext ctx, ITaskExecutionContext execCtx)
    {
        ctx.RemoteServerMutationProxy.SetConnectionStatus(new DefaultConnectionStatusContext(ConnectionState.Handshaking));
        execCtx.Status.Value = "Handshaking";
        
        ClientSoftwareInfo? clientSoftware = ctx.ServiceProvider.GetService<ClientSoftwareInfo>();
        if (clientSoftware == null)
        {
            throw new NullReferenceException(nameof(clientSoftware));
        }

        var modules = ctx.ServiceProvider.GetRequiredService<ModuleStorage<IUniScanClientModule, UniScanClientModuleInitializationArgs>>();
        var serverSoftware = await ctx.RemoteServer.Socket.SendRequestAsync(ClientSoftwareInfoPacket.CreateRequest(clientSoftware, modules.Modules.ToDictionary(kvp => kvp.Key, kvp => new ModuleInfo(kvp.Value.Id))), execCtx.CancellationToken);
        if (serverSoftware.HasValue)
        {
            ctx.RemoteServerMutationProxy.SetSoftwareInfo(serverSoftware.Value.Info);
            _logger.Information("Received server software info: {Info}", serverSoftware.Value.Info);
        }
        else
        {
            ctx.RemoteServerMutationProxy.SetConnectionStatus(new KickedDisconnectedConnectionStatusContext(new TranslatableTextComponent("UniScan:client/i18n/disconnect_reason/handshake_rejected")));
            
            _logger.Error(serverSoftware.Error, "Server rejected handshake");
            throw new Exception("Server rejected handshake");
        }
    }

    public async Task GetDeviceList(TaskContexts.RemoteContext ctx, ITaskExecutionContext execCtx)
    {
        ctx.RemoteServerMutationProxy.SetConnectionStatus(new DefaultConnectionStatusContext(ConnectionState.Connected));
        execCtx.Status.Value = "Receiving devices";
        
        var devices = await ctx.RemoteServer.Socket.SendRequestAsync(GetDeviceListPacket.CreateRequest(), execCtx.CancellationToken);

        if (devices.HasValue)
        {
            _logger.Debug("Received devices list: [{Devices}]", string.Join(", ", devices.Value.Devices));

            ctx.RemoteServer.Devices.Clear();
            foreach (var deviceInfoDto in devices.Value.Devices)
            {
                ctx.RemoteServer.Devices.Add(deviceInfoDto.Key, RemoteDevice.FromDto(deviceInfoDto.Value, ctx.RemoteServer));    
            }
        }
        else
        {
            Log.Error(devices.Error, "Exception returned when trying to get devices list");
        }
    }
}