using System.Reflection;
using Serilog;
using Shiki.Common.Collections;
using Shiki.Common.Identity;
using Shiki.Common.Util;

namespace UniScan.Network;

public class PacketRegistry
{
    //global instance that can be shared
    public static PacketRegistry Instance { get; } = new();
    
    private readonly ILogger _logger = Log.Logger.ForContext<PacketRegistry>();
    
    private readonly BiDictionary<Identifier, Type> _packets = new();
    
    public int Count => this._packets.Count;
    
    public Type this[Identifier identifier] => _packets[identifier];

    public Type? GetPacketType(Identifier identifier) => _packets.GetValueOrDefaultFromPrimary(identifier);
    public Identifier? GetIdentifier(Type packetType) => _packets.GetValueOrDefaultFromReverse(packetType);

    public void Register<TPacket>()
        where TPacket : IPacket
        => Register(typeof(TPacket));

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