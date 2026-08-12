using UniScan.Device.Connection.Protocol.Command;

namespace UniScan.Device.Connection.Protocol.Payload;

public interface ICommandPayload<TOpCode>
    where TOpCode : notnull
{
    /// <summary>
    /// The definition for this command payload
    /// </summary>
    public static abstract ICommandDefinition<TOpCode> CommandDefinition { get; }
}

public interface ICommandPayload<TOpCode, TPayload> : ICommandPayload<TOpCode>
    where TPayload : IScannerPayload, ICommandPayload<TOpCode, TPayload>
    where TOpCode : notnull
{
    /// <summary>
    /// The definition for this command payload
    /// </summary>
    public new static abstract CommandDefinition<TOpCode, TPayload> CommandDefinition { get; }

    /// <inheritdoc/>
    static ICommandDefinition<TOpCode> ICommandPayload<TOpCode>.CommandDefinition => TPayload.CommandDefinition;
}