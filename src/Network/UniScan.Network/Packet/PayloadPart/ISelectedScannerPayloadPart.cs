using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;

namespace UniScan.Network.Packet.PayloadPart;

/// <summary>
/// A payload part meant to be sent with device commands to decide which device to run an action on
/// Also sent by the server with device data payloads
///
/// When authenticating and checking permissions, check using the ID given here.
/// </summary>
public interface ISelectedScannerPayloadPart
{
    // TODO turn into Identifier and require an Identifier on each Host, move Name from Scanner to Host if not already done
    // They should consist of a display name and internal ID
    
    public abstract Slug<SnakeSlugFormatter> ScannerIdentifier { get; }
};