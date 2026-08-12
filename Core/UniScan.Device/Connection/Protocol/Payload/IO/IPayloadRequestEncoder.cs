namespace UniScan.Device.Connection.Protocol.Payload.IO;

public interface IPayloadRequestEncoder<TOpCode, in TArgs> : ICommandPayload<TOpCode>
    where TOpCode : notnull
{
    /// <summary>
    /// Encodes a request to fetch the ScannerPayload
    /// </summary>
    /// <returns>The request byte array</returns>
    public static abstract byte[] EncodeRequest(TArgs args);
}