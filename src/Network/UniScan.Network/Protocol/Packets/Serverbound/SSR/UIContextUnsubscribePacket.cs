using System.ComponentModel;
using MessagePack;
using Shiki.Common.Identity;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.Packets.Clientbound;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;
using UniScan.Network.Request;

namespace UniScan.Network.Protocol.Packets.Serverbound.SSR;

/// <summary>
/// Unsubscribes from a UI context, telling the server to stop sending data from this context
/// </summary>
[RegistryPacket("UniScan", "packet", "clientbound", "ssr", "ui_context_unsubscribe")]
[MessagePackObject]
[method: RequestConstructor]
public readonly partial record struct UIContextUnsubscribePacket(
    [property: Key(0)] Identifier Identifier,
    [property: Key(1)] Guid? RequestId
) : IServerboundPacket, IRequestPayloadPart<AcknowledgePacket>;