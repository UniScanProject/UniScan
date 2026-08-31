using Shiki.Common.Identity;

namespace UniScan.Core.State.Node.Nodes;

/// <summary>
/// Already serialized state node containing the state identifier and the serialized data
///
/// Used by the client to store states it does not have registered but may need to still send back to the server.
/// </summary>
[StateNode("UniScan:device/state/node/serialized")]
public class SerializedStateNode(Identifier id, byte[] data) : IDeviceStateNode
{
    public Identifier Identifier { get; } = id;
    public byte[] Data { get; } = data;
}