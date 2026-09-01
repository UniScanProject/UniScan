using System.Collections.Concurrent;
using DotNetty.Transport.Channels;

namespace UniScan.Network.Server;

public class SubscribableGroup
{
    private readonly ConcurrentDictionary<IChannelId, IChannel> _channels = new();
    
    public int Count => _channels.Count;

    public void Add(IChannel channel)
    {
        if (_channels.TryAdd(channel.Id, channel))
        {
            channel.CloseCompletion.ContinueWith(_ => Remove(channel));
        }
    }

    public void Remove(IChannel channel) => _channels.TryRemove(channel.Id, out _);
    
    public bool Contains(IChannel channel) => _channels.ContainsKey(channel.Id);

    public Task BroadcastAsync<TPacket>(TPacket packet) where TPacket : IPacket =>
         _channels.IsEmpty ? Task.CompletedTask : Task.WhenAll(_channels.Values.Select(c => c.WriteAndFlushAsync(packet)));

    public async Task CloseAllAsync()
    {
        await Task.WhenAll(_channels.Values.Select(c => c.CloseAsync()));
        _channels.Clear();
    }
}