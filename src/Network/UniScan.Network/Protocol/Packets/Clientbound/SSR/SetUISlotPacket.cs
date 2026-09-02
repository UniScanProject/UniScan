using MessagePack;
using Shiki.Common.Identity;
using UniScan.Network.Registry;
using UniScan.UserInterface;

namespace UniScan.Network.Protocol.Packets.Clientbound.SSR;

/// <summary>
/// Sent to clients to set a UISlot
/// </summary>
[RegistryPacket("UniScan", "packet", "clientbound", "ssr", "set_ui_slot")]
[MessagePackObject]
public readonly record struct SetUISlotPacket(
    [property: Key(0)] Identifier SlotIdentifier,
    [property: Key(1)] IUINode? Node
) : IClientboundPacket;