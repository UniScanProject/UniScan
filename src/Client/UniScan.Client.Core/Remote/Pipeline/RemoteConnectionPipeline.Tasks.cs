using Microsoft.Extensions.DependencyInjection;
using Serilog;
using UniScan.Client.Core.Remote.Device;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Protocol.Packets.Serverbound;
using UniScan.Network.Protocol.Packets.Serverbound.Client;

namespace UniScan.Client.Core.Remote.Pipeline;

public partial class RemoteConnectionPipeline
{
    public async Task StartConnection(TaskContexts.ConnectionContext ctx, CancellationToken ct = default)
    {
        ctx.Status.Value = $"Connecting to {ctx.RemoteServer.ConnectionMethod.ToDisplayString()}";
        
        await ctx.RemoteServer.Socket.StartAsync();
        if (!ctx.RemoteServer.Socket.Connected)
        {
            throw new InvalidOperationException("Connection failed!");
        }
        
        ctx.RemoteServer.Socket.Channel?.GetAttribute(ServerAttributes.ServerAttribute).Set(ctx.RemoteServer);
    }
    
    public async Task Handshake(TaskContexts.NegotiationContext ctx, CancellationToken ct = default)
    {
        ctx.Status.Value = $"Handshaking";
        
        ClientSoftwareInfo? clientSoftware = ctx.ServiceProvider.GetService<ClientSoftwareInfo>();
        if (clientSoftware == null)
        {
            throw new NullReferenceException(nameof(clientSoftware));
        }
        
        var serverSoftware = await ctx.RemoteServer.Socket.SendRequestAsync(ClientSoftwareInfoPacket.CreateRequest(clientSoftware), ct);
        if (serverSoftware.HasValue)
        {
            ctx.RemoteServerMutationProxy.SetSoftwareInfo(serverSoftware.Value.Info);
            _logger.Information("Received server software info: {Info}", serverSoftware.Value.Info);
        }
        else
        {
            _logger.Error(serverSoftware.Error, "Server rejected handshake");
            throw new Exception("Server rejected handshake");
        }
    }

    public async Task GetDeviceList(TaskContexts.RemoteContext ctx, CancellationToken ct = default)
    {
        ctx.Status.Value = "Receiving devices";

        
        var devices = await ctx.RemoteServer.Socket.SendRequestAsync(GetDeviceListPacket.CreateRequest(), ct);

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