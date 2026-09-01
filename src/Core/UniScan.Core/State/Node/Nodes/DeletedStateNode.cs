namespace UniScan.Core.State.Node.Nodes;

/// <summary>
/// Used to signal that the data for whatever key this was sent on behalf of should be removed
/// </summary>
[StateNode("UniScan:device/state/node/deleted")]
public class DeletedStateNode : IDeviceStateNode
{
    
}