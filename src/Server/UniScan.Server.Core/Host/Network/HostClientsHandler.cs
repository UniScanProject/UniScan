using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using R3;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Util;
using UniScan.Core.State;
using UniScan.Core.State.Node;
using UniScan.Device.Device;
using UniScan.Network.Protocol.Packets.Clientbound.Device;
using UniScan.Network.Server;

namespace UniScan.Server.Core.Host.Network;

public sealed class HostClientsHandler : IDisposable, IAsyncDisposable
{
    private readonly Slug<SnakeSlugFormatter> _scannerId;
    private readonly Scanner _scanner;
    private DeviceState? _currentState;

    private readonly SubscribableGroup _subscribers = new();

    private IDisposable _disposable;

    public HostClientsHandler(Slug<SnakeSlugFormatter> scannerId, Scanner scanner)
    {
        _scannerId = scannerId;
        _scanner = scanner;
        
        _disposable = _scanner.State.AsObservable().Subscribe(HandleState);
    }

    private void HandleState(DeviceState? state)
    {
        _currentState = state;
        if (state == null)
            return;
        
        // TODO it will be better to get difference of entire state and ship that out but atm this is best I can do
        //every 10 state updates lets send full state too to avoid desync
        DeviceStatePacket packet = new(DeviceStateSerializer.Serialize(state), null, _scannerId);
        
        _ = _subscribers.BroadcastAsync(packet);
    }
    
    public void AddClient(IChannel channel) => _subscribers.Add(channel);

    public bool Contains(IChannel channel) => _subscribers.Contains(channel);

    public void Dispose()
    {
        _disposable.Dispose();
        _ = _subscribers.CloseAllAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _disposable.Dispose();
        await _subscribers.CloseAllAsync();
    }
}