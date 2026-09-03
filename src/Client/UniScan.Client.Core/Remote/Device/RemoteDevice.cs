using ObservableCollections;
using R3;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Core.State.Node;
using UniScan.Network.Data;
using UniScan.Network.Data.Device;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.Packets.Serverbound.Subscription;

namespace UniScan.Client.Core.Remote.Device;

public class RemoteDevice(Slug<SnakeSlugFormatter> id, string? name, bool connected, DeviceSpecifications? specs, RemoteServer parent) : IDisposable
{
    public Slug<SnakeSlugFormatter> Identifier { get; } = id;

    public string? DisplayName { get; } = name;
    public bool Connected { get; } = connected;
    
    private readonly BindableReactiveProperty<bool> _subscribed = new(false);
    public IReadOnlyBindableReactiveProperty<bool> Subscribed => _subscribed;
    
    public DeviceSpecifications? Specs { get; } = specs;

    public ObservableDictionary<Identifier, IDeviceStateNode> States { get; set; } = [];
    
    public static RemoteDevice FromDto(DeviceDto dto, RemoteServer server) => new(dto.DeviceIdentifier, dto.DisplayName, dto.Connected, dto.Specs, server);

    public async Task<bool> Subscribe()
    {
        _subscribed.Value = (await parent.Socket.SendRequestAsync<AcknowledgePacket>(SubscribePacket.CreateRequest(Identifier))).Value.Result.Success;

        return _subscribed.CurrentValue;
    }

    public void Dispose()
    {
        _subscribed.Dispose();
    }
}