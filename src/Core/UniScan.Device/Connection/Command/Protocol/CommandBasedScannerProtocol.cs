using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Serilog;
using UniScan.Device.Connection.Protocol;
using UniScan.Device.Connection.Protocol.Payload;
using UniScan.Device.Connection.Protocol.Payload.IO.Exception;

namespace UniScan.Device.Connection.Command.Protocol;

public class CommandPacketReceivedEventArgs<TOpCode>(TOpCode opCode, IScannerPayload payload) : PacketReceivedEventArgs<IScannerPayload>(payload)
where TOpCode : notnull
{
    public TOpCode OpCode { get; } = opCode;
}

/// <summary>
/// <inheritdoc/>
/// 
/// <br />
/// 
/// This base implementation provides helpers for command-based protocols
/// </summary>
public abstract class CommandBasedScannerProtocol<TOpCode> : IScannerProtocol<IScannerPayload>
    where TOpCode : notnull
{
    /// <inheritdoc/>
    public event EventHandler<PacketReceivedEventArgs<IScannerPayload>>? PacketReceived;
    public event EventHandler<CommandPacketReceivedEventArgs<TOpCode>>? CommandPacketReceived;

    /// <summary>
    /// Registry of opcode -> command definitions, used for decoding payloads into their classes
    /// </summary>
    private readonly Dictionary<TOpCode, ICommandDefinition<TOpCode>> _definitionRegistry = new();

    /// <inheritdoc/>
    public ILogger Logger { get; set; } = Log.Logger.ForContext<CommandBasedScannerProtocol<TOpCode>>();

    public CommandBasedScannerProtocol()
    {
        this.CommandPacketReceived += (sender, args) =>
        {
            this.PacketReceived?.Invoke(this, args);
        };
    }
    
    /// <summary>
    /// Registers a command definition
    /// </summary>
    /// <typeparam name="T">The payload type</typeparam>
    protected void RegisterDefinition<T>() where T : ICommandPayload<TOpCode> =>
        _definitionRegistry[T.CommandDefinition.OpCode] = T.CommandDefinition;

    /// <inheritdoc/>
    public bool HandlePayload(ref ReadOnlySequence<byte> buffer)
    {
        //nothing to read if true
        if (buffer.IsEmpty)
            return false;

        // get our opcode, we use this to get the definition which contains the decoder
        if (!TryGetOpCode(buffer, out TOpCode? opcode))
            return false;

        //fetch definition with our opcode
        if (_definitionRegistry.TryGetValue(opcode, out var definition))
        {
            var v = definition.Parser(buffer, out SequencePosition endPosition);
            switch (v.Error)
            {
                case PayloadParseError.None: {
                    CommandPacketReceived?.Invoke(this, new CommandPacketReceivedEventArgs<TOpCode>(opcode, v.Value!));
                    buffer = buffer.Slice(endPosition);
                    return true;
                }
                case PayloadParseError.Incomplete: return false;
     
                default: return TrySkipPayload(ref buffer);
            }
        }

        //we didn't find a definition with this opcode, not good.
        Logger.Warning("Received unknown OpCode: {OpCode}", opcode);
        return TrySkipPayload(ref buffer);
    }

    /// <inheritdoc/>
    public bool TrySkipPayload(ref ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);
        if (!reader.TryReadTo(out ReadOnlySequence<byte> _, (byte)'\r', advancePastDelimiter: true))
            return false;

        buffer = buffer.Slice(reader.Position);
        return true;
    }

    /// <summary>
    /// Attempts to get the OpCode in the current buffer
    /// </summary>
    /// <param name="buffer">The buffer</param>
    /// <param name="opCode">The resulting opcode</param>
    /// <returns>Whether the OpCode was extracted</returns>
    protected abstract bool TryGetOpCode(ReadOnlySequence<byte> buffer,
                                         [NotNullWhen(returnValue: true)] out TOpCode? opCode);
}