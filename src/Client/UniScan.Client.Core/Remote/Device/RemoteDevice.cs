using ObservableCollections;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Core.State.Node;
using UniScan.Network.Data;

namespace UniScan.Client.Core.Remote.Device;

public class RemoteDevice(Slug<SnakeSlugFormatter> id, string? name, bool connected, DeviceSpecifications? specs)
{
    public Slug<SnakeSlugFormatter> Identifier { get; } = id;

    public string? DisplayName { get; } = name;
    public bool Connected { get; } = connected;
    public DeviceSpecifications? Specs { get; } = specs;

    public ObservableDictionary<Identifier, IDeviceStateNode> States { get; set; } = [];
    
    public static RemoteDevice FromDto(DeviceDto dto) => new(dto.ScannerIdentifier, dto.DisplayName, dto.Connected, dto.Specs);
}