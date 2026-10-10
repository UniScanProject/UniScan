using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using R3;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Util;
using Shiki.Internationalization.Component;
using UniScan.Core.State;
using UniScan.Core.State.Node;
using UniScan.Device.Device;
using UniScan.Network.Protocol.Packets.Clientbound.Device;
using UniScan.Network.Server;

namespace UniScan.Server.Core.Host.Network;

public sealed class HostClientsHandler : IDisposable, IAsyncDisposable
{
    private readonly Slug<SnakeSlugFormatter> _deviceId;
    private readonly Device.Device.Device _device;
    private IDeviceState? _currentState;
    private Dictionary<Identifier, IStateNode> _currentStates = [];

    private readonly SubscribableGroup _subscribers = new();

    private readonly CompositeDisposable _disposables = new();

    public HostClientsHandler(Slug<SnakeSlugFormatter> deviceId, Device.Device.Device device)
    {
        _deviceId = deviceId;
        _device = device;
        
        _device.State.AsObservable().DistinctUntilChanged().Subscribe(HandleState).AddTo(_disposables);
        _device.Active.AsObservable().DistinctUntilChanged().Subscribe(HandleActiveChanged).AddTo(_disposables);
    }

    private void HandleState(IDeviceState? state)
    {
        _currentState = state;
        if (state == null)
            return;

        var serialized = StateSerializer.SerializeContainer(state);
        Dictionary<Identifier, IStateNode?> diff = [];

        foreach (var kvp in serialized)
        {
            if (!_currentStates.TryGetValue(kvp.Key, out IStateNode? node) || !Equals(node, kvp.Value))
            {
                diff[kvp.Key] = kvp.Value;
            }
        }

        foreach (Identifier k in _currentStates.Keys)
        {
            if (!serialized.ContainsKey(k))
            {
                diff[k] = null;
            }
        }

        if (diff.Count == 0)
            return;

        _currentStates = serialized;
        
        //TODO maybe every 10 state updates lets send full state too to avoid desync
        StatePacket packet = new(diff, null, _deviceId);
        
        _ = _subscribers.BroadcastAsync(packet);
    }

    public void HandleActiveChanged(bool active)
    {
        if (!active)
        {
            foreach ((IChannelId id, IChannel channel) in _subscribers.Channels)
            {
                _subscribers.Remove(id);
                channel.WriteAndFlushAsync(new UnsubscribeFromDevicePacket(_deviceId,
                                                                           new TranslatableTextComponent(Identifier
                                                                              .CreateInstance("UniScan:i18n/device/disconnected_inactive"))));
            }
        }
    }

    public void AddClient(IChannel channel)
    {
        _subscribers.Add(channel);

        if (_currentStates.Count != 0)
        {
            //init states on the client on first connect
            channel.WriteAndFlushAsync(new StatePacket(_currentStates.ToDictionary(kvp => kvp.Key, IStateNode? (kvp) => kvp.Value), null,
                                                       _deviceId));
        }
    } 

    public bool Contains(IChannel channel) => _subscribers.Contains(channel);

    public void Dispose()
    {
        _disposables.Dispose();
        _ = _subscribers.CloseAllAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _disposables.Dispose();
        await _subscribers.CloseAllAsync();
    }
}