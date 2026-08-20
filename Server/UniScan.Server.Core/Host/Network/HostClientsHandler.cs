using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Util;
using UniScan.Core.State;
using UniScan.Device.Device;
using UniScan.Network.Packet.Packets.Clientbound.Device;
using UniScan.Network.Server;

namespace UniScan.Server.Core.Host.Network;

public sealed class HostClientsHandler : IDisposable, IAsyncDisposable
{
    private readonly Slug<SnakeSlugFormatter> _scannerId;
    private readonly Scanner _scanner;

    private readonly SubscribableGroup _subscribers = new();

    public HostClientsHandler(Slug<SnakeSlugFormatter> scannerId, Scanner scanner)
    {
        _scannerId = scannerId;
        _scanner = scanner;
        
        _scanner.OnStateUpdated += HandleState;
    }

    private void HandleState(DeviceState state)
    {
        // TODO it will be better to get difference of entire state and ship that out but atm this is best I can do
        //every 10 state updates lets send full state too to avoid desync
        StatePacket packet = new(state, null, _scannerId);
        
        _ = _subscribers.BroadcastAsync(packet);
    }
    
    public void AddClient(IChannel channel) => _subscribers.Add(channel);

    public void Dispose()
    {
        _scanner.OnStateUpdated -= HandleState;
        _ = _subscribers.CloseAllAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _scanner.OnStateUpdated -= HandleState;
        await _subscribers.CloseAllAsync();
    }
}