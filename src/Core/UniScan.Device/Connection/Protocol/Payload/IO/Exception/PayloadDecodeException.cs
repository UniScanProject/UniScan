namespace UniScan.Device.Connection.Protocol.Payload.IO.Exception;

public class PayloadDecodeException : IOException
{
    public Type DecoderType { get; }

    public PayloadDecodeException(Type type, string msg) : base($"Failed to decode in {type.FullName}: {msg}")
    {
        if (!typeof(IDevicePayload).IsAssignableFrom(type))
            throw new ArgumentException($"{type.FullName} is not a DevicePayload");

        this.DecoderType = type;
    }
}