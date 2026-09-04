using System.Reflection;
using Serilog;
using Shiki.Common.Collections;
using Shiki.Common.Identity;
using UniScan.Network.Registry.Source;

namespace UniScan.Network.Registry;

public class PacketRegistry
{
    private readonly ILogger _logger = Log.ForContext<PacketRegistry>();

    private readonly BiDictionary<Identifier, Type> _packets = new();

    public int Count => this._packets.Count;

    public Type this[Identifier identifier] => _packets[identifier];

    public Type? GetPacketType(Identifier identifier) => _packets.GetValueOrDefaultFromPrimary(identifier);
    public Identifier? GetIdentifier(Type packetType) => _packets.GetValueOrDefaultFromReverse(packetType);

    public void Register<TPacket>()
        where TPacket : IPacket
        => Register(typeof(TPacket));

    public void RegisterFromSource<TPacketSource>()
        where TPacketSource : IPacketSource, new() => RegisterFromSource(new TPacketSource());

    public void RegisterFromSource(IPacketSource packetSource)
    {
        foreach ((Type pk, RegistryPacketAttribute? rpk) in packetSource.GetPacketTypes())
            Register(pk, rpk);
    }


    public void Register(Type packetType)
    {
        RegistryPacketAttribute? p = packetType.GetCustomAttribute<RegistryPacketAttribute>();
        if (p == null)
        {
            throw new
                ArgumentException($"Attempted to register packet with type {packetType.FullName}, however required attribute {typeof(RegistryPacketAttribute).FullName} is NOT present");
        }

        this.Register(packetType, p);
    }

    private void Register(Type packet, RegistryPacketAttribute attr)
    {
        ArgumentNullException.ThrowIfNull(attr);

        if (_packets.PrimaryContainsKey(attr.Id))
        {
            throw new
                InvalidOperationException($"Attempted to register packet of type '{packet.FullName}' with id '{attr.Id}' that is already registered by packet with type '{_packets[attr.Id].FullName}'");
        }
            
        _packets.Add(attr.Id, packet);
        _logger.Information("Registered packet type {Id}", attr.Id);
    }
}