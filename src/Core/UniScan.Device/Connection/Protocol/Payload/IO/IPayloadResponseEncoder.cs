namespace UniScan.Device.Connection.Protocol.Payload.IO;

public interface IEncodable
{
    /// <summary>
    /// Encodes the structure the same way the scanner would encode it
    /// </summary>
    /// <returns>The encoded structure byte array</returns>
    public byte[] EncodeResponse();
}

public interface IPayloadResponseEncoder<TOpCode> : IEncodable, ICommandPayload<TOpCode>
    where TOpCode : notnull
{

}