using System;
using System.Threading.Tasks;
using Shiki.Common.Identity;
using UniScan.Network.Data.SSR;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.Packets.Serverbound.SSR;

namespace UniScan.Network.Client.Serialization;

public static class UIContextSubscriptionManager
{
    public interface ISubscription : IDisposable, IAsyncDisposable;
    
    private class Subscription(Identifier contextId, ClientSocket socket) : ISubscription
    {
        public void Dispose()
        {
            socket.SendRequestAsync<AcknowledgePacket>(UIContextUnsubscribePacket.CreateRequest(contextId));
        }

        public async ValueTask DisposeAsync()
        {
            await socket.SendRequestAsync<AcknowledgePacket>(UIContextUnsubscribePacket.CreateRequest(contextId));
        }
    }

    public static async Task<ISubscription?> SubscribeAsync(ClientSocket socket, Identifier contextId, IUISubscribeArgs? args) => await socket.SendRequestAsync<AcknowledgePacket>(UIContextSubscribePacket.CreateRequest(contextId, args)) is { HasValue: true, Value.Result.IsSuccessful: true } ? new Subscription(contextId, socket) : null;
}

public static class ClientSocketExtensions
{
    extension(ClientSocket socket)
    {
        public async Task<UIContextSubscriptionManager.ISubscription?> SubscribeToUIContextAsync(Identifier contextId, IUISubscribeArgs? args) => await UIContextSubscriptionManager.SubscribeAsync(socket, contextId, args);
    }
}