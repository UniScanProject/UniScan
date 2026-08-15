using System.Buffers;
using Shiki.Common.Result;
using UniScan.Device.Connection.Protocol.Payload;
using UniScan.Device.Connection.Protocol.Payload.IO;
using UniScan.Device.Connection.Protocol.Payload.IO.Exception;

namespace UniScan.Device.Connection.Command.Protocol;

public interface ICommandDefinition<TOpCode>
    where TOpCode : notnull
{
    public TOpCode OpCode { get; }
    public ReadOnlyMemory<byte> OpCodeBytes { get; }
    public Type Type { get; }
    public IPayloadParser<IScannerPayload>.ParseDelegate Parser { get; }
}

public readonly struct CommandDefinition<TOpCode, TType>(TOpCode opCode, IPayloadParser<TType>.ParseDelegate parser) : ICommandDefinition<TOpCode>
    where TOpCode : notnull
    where TType : IScannerPayload
{
    public TOpCode OpCode { get; } = opCode;
    public required ReadOnlyMemory<byte> OpCodeBytes { get; init; }
    
    public Type Type { get; } = typeof(TType);
    public IPayloadParser<TType>.ParseDelegate Parser { get; } = parser;

    IPayloadParser<IScannerPayload>.ParseDelegate ICommandDefinition<TOpCode>.Parser => Parse;

    private ErrorCodeResult<IScannerPayload, PayloadParseError> Parse(ReadOnlySequence<byte> buffer,
                                                                             out SequencePosition consumed)
    {
        var v = Parser(buffer, out consumed);
        if (v.HasValue)
        {
            return v.Value;
        }
        
        return v.Error;
    }
}