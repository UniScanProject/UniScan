using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;

namespace UniScan.Network.Protocol.PayloadPart;

/// <summary>
/// A payload part meant to be sent with device commands to decide which device to run an action on
/// Also sent by the server with device data payloads
///
/// When authenticating and checking permissions, check using the ID given here.
/// </summary>
public interface ISelectedDevicePayloadPart
{
    public abstract Slug<SnakeSlugFormatter> DeviceIdentifier { get; }
};