using System.Reflection;
using Serilog;
using Shiki.Common.Collections;
using Shiki.Common.Identity;
using Shiki.Common.Util;
using UniScan.Network.Registry;
using UniScan.Network.Registry.Source;

namespace UniScan.Network;

public class PacketRegistry
{
    private readonly ILogger _logger = Log.Logger.ForContext<PacketRegistry>();

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
        {
            if (rpk == null)
            {
                throw new
                    ArgumentException($"Attempted to register packet with type {pk.FullName}, however required attribute {typeof(RegistryPacketAttribute).FullName} is NOT present");
            }

            if (_packets.PrimaryContainsKey(rpk.Id))
            {
                throw new
                    InvalidOperationException($"Attempted to register packet of type '{pk.FullName}' with id '{rpk.Id}' that is already registered by packet with type '{_packets[rpk.Id].FullName}'");
            }
            
            _packets.Add(rpk.Id, pk);
            _logger.Information("Registered packet type {Id}", rpk.Id);
        }
    }


    public void Register(Type packetType)
    {
        RegistryPacketAttribute? p = packetType.GetCustomAttribute<RegistryPacketAttribute>();
        if (p == null)
        {
            throw new
                ArgumentException($"Attempted to register packet with type {packetType.FullName}, however required attribute {typeof(RegistryPacketAttribute).FullName} is NOT present");
        }

        this._packets.Add(p.Id, packetType);
        _logger.Information("Registered packet type {Id}", p.Id);
    }
}