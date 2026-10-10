using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using Shiki.Common.Identity;
using Shiki.Common.Util;
using UniScan.Core.State.Attribute;
using UniScan.Core.State.Node;
using UniScan.Core.State.Node.Nodes;
using UniScan.Core.State.Radio;
using UniScan.Core.State.Types;

namespace UniScan.Core.State;

/// <summary>
/// The state stored within a Scanner, and converted from received data from the remote.
///
/// To be passed over the network to all clients
/// </summary>
public interface IDeviceState;