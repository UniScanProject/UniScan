using System.Collections.Immutable;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using Serilog;
using UniScan.Network;
using UniScan.Network.Group;
using UniScan.Network.Socket;

namespace UniScan.Audio.Server;

public class AudioHostSocket(
    UniScanChannelInitializer channelInitializer,
    IEnumerable<IAudioHostMethod> methods,
    IHostNetworkGroupManager groupManager)
    : ISocket
{
    public ILogger Logger { get; } = Log.ForContext<AudioHostSocket>();

    public UniScanChannelInitializer ChannelInitializer { get; } = channelInitializer;

    private readonly ImmutableList<IAudioHostMethod> _hostMethods = [.. methods];
    private readonly List<IChannel> _channels = [];

    public async Task StartAsync()
    {
        if (_hostMethods.IsEmpty)
        {
            throw new Exception("No host methods found");
        }
        
        try
        {
            var task = _hostMethods.Select(async m =>
            {
                IChannel c = await m.BindAsync(groupManager, ChannelInitializer);
                Logger.Information("Audio hosting on transport {Transport}", m.ToDisplayString());

                return c;
            });

            _channels.AddRange(await Task.WhenAll(task));
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to start audio host");
            await StopAsync();
        }
    }
    
    public async Task StopAsync()
    {
        var t = _channels.Select(c => c.CloseAsync());
        await Task.WhenAll(t);
        
        _channels.Clear();
    }

    public async Task<bool> SendPacketAsync(IChannel? client, IPacket packet)
    {
        if (client is not { Active: true }) return false;

        await client.WriteAndFlushAsync(packet);
        return true;
    }
}