using ObservableCollections;
using R3;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Result;
using UniScan.Core.State.Node;
using UniScan.Network.Data;
using UniScan.Network.Data.Device;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.Packets.Serverbound.Device;
using UniScan.Network.Protocol.Packets.Serverbound.Subscription;

namespace UniScan.Client.Core.Remote.Device;

public class RemoteDevice(Slug<SnakeSlugFormatter> id, string? name, bool connected, DeviceSpecifications? specs, RemoteServer parent) : IDisposable
{
    public Slug<SnakeSlugFormatter> Identifier { get; } = id;

    public string? DisplayName { get; private set; } = name;
    public string Name => DisplayName ?? Identifier.Value;
    
    public bool Connected { get; private set; } = connected;
    
    public RemoteServer Parent { get; } = parent;
    
    private readonly BindableReactiveProperty<bool> _subscribed = new(false);
    public IReadOnlyBindableReactiveProperty<bool> Subscribed => _subscribed;
    
    public DeviceSpecifications? Specs { get; private set; } = specs;

    public ObservableDictionary<Identifier, IStateNode> States { get; set; } = [];
    
    public static RemoteDevice FromDto(DeviceDto dto, RemoteServer server) => new(dto.DeviceIdentifier, dto.DisplayName, dto.Connected, dto.Specs, server);

    public async ValueTask<BooleanResult<Exception>> Subscribe()
    {
        var res = await parent.Socket.SendRequestAsync<AcknowledgePacket>(DeviceSubscribePacket.CreateRequest(Identifier));
        
        _subscribed.Value = res.Value.Result.IsSuccessful;
        return !res.Value.Result.IsSuccessful ? new BooleanResult<Exception>(res.Value.Result.Error) : BooleanResult<Exception>.Successful;
    }

    public async Task UnsubscribeAsync(bool notify)//todo this is just shit
    {
        if (notify)
        {
            await parent.Socket.SendRequestAsync(DeviceUnsubscribePacket.CreateRequest(Identifier));
        }

        _subscribed.Value = false;
    }

    public void Dispose()
    {
        _subscribed.Dispose();
    }

    //TODO there has to be better way than this but rushing...
    public void UpdateFrom(DeviceDto dto)
    {
        DisplayName = dto.DisplayName;
        Connected = dto.Connected;
        Specs = dto.Specs;
    }
}