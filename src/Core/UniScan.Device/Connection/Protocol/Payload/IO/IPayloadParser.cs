using System.Buffers;
using Shiki.Common.Result;
using UniScan.Device.Connection.Protocol.Payload.IO.Exception;

namespace UniScan.Device.Connection.Protocol.Payload.IO;

public interface IPayloadParser<TSelf> where TSelf : IScannerPayload
{
    public delegate ErrorCodeResult<TSelf, PayloadParseError> ParseDelegate(
        ReadOnlySequence<byte> buffer, out SequencePosition consumed);
    
    /// <summary>
    /// Decodes the next value in the buffer into a ScannerPayload
    /// </summary>
    /// <param name="buffer">The buffer to decode from</param>
    /// <param name="consumed">The position in the buffer after the parser has run</param>
    /// 
    /// <returns>The result</returns>
    public static abstract ErrorCodeResult<TSelf, PayloadParseError> Parse(ReadOnlySequence<byte> buffer, out SequencePosition consumed);
}