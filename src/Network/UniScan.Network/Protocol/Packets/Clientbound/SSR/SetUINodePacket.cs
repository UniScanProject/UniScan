using MessagePack;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using UniScan.Network.Registry;
using UniScan.UserInterface;
using UniScan.UserInterface.Nodes.Core.Node;

namespace UniScan.Network.Protocol.Packets.Clientbound.SSR;

/// <summary>
/// Sent to clients to set a node with the given id to the new given node
/// </summary>
[RegistryPacket("UniScan", "packet", "clientbound", "ssr", "set_ui_node")]
[MessagePackObject]
public readonly record struct SetUINodePacket(
    [property: Key(0)] Identifier SlotIdentifier,
    [property: Key(1)] Slug<DashSlugFormatter> NodeIdentifier,
    [property: Key(2)] IUINode? Node
) : IClientboundPacket
{
    public SetUINodePacket(Identifier slotIdentifier, IUINode node) : this(slotIdentifier, node.Id, node) {}
}