using System.Text.Json.Serialization;
using Shiki.Common.Result;
using UniScan.Core.Util;
using UniScan.Device.Connection.Command.Protocol;
using UniScan.Device.Connection.Protocol.Payload;
using UniScan.Device.Connection.Protocol.Payload.IO;

namespace UniScan.Device.Connection.Command;

public interface ICommandBasedScannerConnection<TOpCode> : IScannerConnection<ReadOnlyMemory<byte>>
    where TOpCode : notnull
{
    [JsonIgnore] public abstract CommandBasedScannerProtocol<TOpCode> Protocol { get; }

    [JsonIgnore]
    public abstract Dictionary<TOpCode, EventHandler<CommandPacketReceivedEventArgs<TOpCode>>> PayloadReceived { get; }

    public Task SendAsync(IPayloadResponseEncoder<TOpCode> encoder);

    public Task SendAsync<TRequestEncoder, TArgs>(TArgs args, CancellationToken ct = default)
        where TRequestEncoder : IPayloadRequestEncoder<TOpCode, TArgs>;

    public async Task SendAsync<TRequestEncoder>(CancellationToken ct = default)
        where TRequestEncoder : IPayloadRequestEncoder<TOpCode, VoidArgument>
        => await SendAsync<TRequestEncoder, VoidArgument>(default, ct);

    public Task<Result<TResponse, Exception>> SendRequestAsync<TRequestEncoder, TArgs, TResponse>(
        TArgs args, CancellationToken ct = default)
        where TRequestEncoder : IPayloadRequestEncoder<TOpCode, TArgs>
        where TResponse : IScannerPayload;

    public Task<Result<TResponse, Exception>> SendRequestAsync<TRequestEncoder, TResponse>(
        CancellationToken ct = default)
        where TRequestEncoder : IPayloadRequestEncoder<TOpCode, VoidArgument>
        where TResponse : IScannerPayload
        => SendRequestAsync<TRequestEncoder, VoidArgument, TResponse>(default, ct);
}