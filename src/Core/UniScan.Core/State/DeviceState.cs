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
[DebuggerDisplay("= Device State =\n- Volume: {Volume}/{MaxVolume}\n- Squelch: {Squelch}/{MaxSquelch}\n- Signal: {Signal}/{MaxSignal}\n- Scanning {ScanDirection}")]
public class DeviceState
{
    /// <summary>
    /// The current volume of the Scanner
    /// </summary>
    [StateProperty<Int32StateNode>("UniScan:device/state/property/volume")]
    public int Volume { get; set; } = 0;
    /// <summary>
    /// The maximum possible volume of the Scanner, todo can we handle this better? I would rather not store it like this...
    /// </summary>
    [StateProperty<Int32StateNode>("UniScan:device/state/property/max_volume")]
    public int MaxVolume { get; set; } = 100;

    /// <summary>
    /// The current squelch of the Scanner
    /// </summary>
    [StateProperty<Int32StateNode>("UniScan:device/state/property/squelch")]
    public int Squelch { get; set; } = 0;
    /// <summary>
    /// The maximum squelch of the Scanner
    /// </summary>
    [StateProperty<Int32StateNode>("UniScan:device/state/property/max_squelch")]
    public int MaxSquelch { get; set; } = 100;

    /// <summary>
    /// The currently received signal strength
    ///
    /// Should be 0 when no data is being received on the current channel
    /// </summary>
    [StateProperty<Int32StateNode>("UniScan:device/state/property/signal")]
    public int Signal { get; set; } = 0;
    /// <summary>
    /// The maximum possible received signal
    /// </summary>
    [StateProperty<Int32StateNode>("UniScan:device/state/property/max_signal")]
    public int MaxSignal { get; set; } = 5;

    /// <summary>
    /// The direction the scanner is scanning in
    /// </summary>
    public ScanDirection? ScanDirection { get; set; }

    /// <summary>
    /// The device's scan tree
    /// </summary>
    private List<ScanList> ScanLists { get; set; }
    
    /// <summary>
    /// The current List being scanned
    /// </summary>
    public ScanList? CurrentScanList { get; set; }
    /// <summary>
    /// The current Zone being scanned
    /// </summary>
    public IScanZone? CurrentZone { get; set; }
    /// <summary>
    /// The current Group being scanned
    /// </summary>
    public IScanGroup? CurrentGroup { get; set; }
    /// <summary>
    /// The current Channel
    /// </summary>
    public IScanChannel? CurrentChannel { get; set; }

    /// <inheritdoc/>
    public override string ToString() => JsonSerializer.Serialize(this);
}