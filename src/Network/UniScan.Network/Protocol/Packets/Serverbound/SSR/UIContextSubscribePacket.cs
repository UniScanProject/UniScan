using MessagePack;
using Shiki.Common.Identity;
using UniScan.Network.Data.SSR;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Protocol.PayloadPart;
using UniScan.Network.Registry;
using UniScan.Network.Request;

namespace UniScan.Network.Protocol.Packets.Serverbound.SSR;

/// <summary>
/// Subscribes to a UI context, tells the server to start sending UI data for this context
/// </summary>
[RegistryPacket("UniScan", "packet", "clientbound", "ssr", "ui_context_subscribe")]
[MessagePackObject]
[method: RequestConstructor]
public readonly partial record struct UIContextSubscribePacket(
    [property: Key(0)] Identifier Identifier,
    [property: Key(1), MessagePackFormatter(typeof(UISubscribeArgsMessagePackFormatter))] IUISubscribeArgs? Args,
    [property: Key(2)] Guid? RequestId
) : IServerboundPacket, IRequestPayloadPart<AcknowledgePacket>;