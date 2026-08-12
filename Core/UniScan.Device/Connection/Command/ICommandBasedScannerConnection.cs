using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Serilog;
using Shiki.Common.Result;
using Shiki.Common.Util;
using UniScan.Core.Util;
using UniScan.Device.Connection.Method;
using UniScan.Device.Connection.Protocol;
using UniScan.Device.Connection.Protocol.Payload;
using UniScan.Device.Connection.Protocol.Payload.IO;

namespace UniScan.Device.Connection;

public interface IScannerConnection
{
    [JsonInclude]
    [JsonPropertyName("method")]
    public ITransport Transport { get; }
    
    [JsonIgnore] protected ILogger Logger { get; }
    
    public Task StartAsync();
    public Task StopAsync();
    
    public Task RunAsync(CancellationToken ct = default);
}

public interface ICommandBasedScannerConnection<TOpCode> : IScannerConnection
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

public abstract class ScannerConnection(ITransport transport) : IScannerConnection
{
    public ITransport Transport { get; } = transport;
    public ILogger Logger { get; }

    public event EventHandler<PacketReceivedEventArgs>? PacketReceived;

    public async Task StartAsync() => await Transport.OpenAsync();

    public async Task StopAsync() => await Transport.CloseAsync();
    
    protected virtual void OnPacketReceived(IScannerPayload payload)
    {
        PacketReceived?.Invoke(this, new PacketReceivedEventArgs(payload));
    }

    public abstract Task RunAsync(CancellationToken ct = default);
}

public abstract class CommandBasedScannerConnection<TOpCode, TProtocol> : ScannerConnection, ICommandBasedScannerConnection<TOpCode>
    where TOpCode : notnull
    where TProtocol : CommandBasedScannerProtocol<TOpCode>
{
    public TProtocol Protocol { get; }
    CommandBasedScannerProtocol<TOpCode> ICommandBasedScannerConnection<TOpCode>.Protocol => Protocol;
    
    public ITransport Transport { get; }
    public ILogger Logger { get; set; } = Log.Logger.ForContext<CommandBasedScannerConnection<TOpCode, TProtocol>>();
    public Dictionary<TOpCode, EventHandler<CommandPacketReceivedEventArgs<TOpCode>>> PayloadReceived { get; } = [];

    private readonly Dictionary<TOpCode, Queue<TaskCompletionSource<IScannerPayload>>> _commandQueues = new();
    private readonly SemaphoreSlim _writeSemaphore = new(1, 1); //my name is slim semaphore
    private readonly Channel<CommandPacketReceivedEventArgs<TOpCode>> _packetDispatcherQueue;
    private Pipe? _bufferPipe;

    protected CommandBasedScannerConnection(TProtocol protocol, ITransport transport) : base(transport)
    {
        Protocol = protocol;
        Transport = transport;

        _packetDispatcherQueue = Channel.CreateUnbounded<CommandPacketReceivedEventArgs<TOpCode>>(new UnboundedChannelOptions
        {
            SingleReader = true,
            AllowSynchronousContinuations = false
        });
        
        Transport.DataReceived += OnTransportDataReceived;
        Protocol.CommandPacketReceived += OnPacketReceived;
    }

    public virtual async Task SendAsync(IPayloadResponseEncoder<TOpCode> encoder)
    {
        await this.Transport.SendAsync(encoder.EncodeResponse());
    }

    public async Task SendAsync<TRequestEncoder, TArgs>(TArgs args, CancellationToken ct = default)
        where TRequestEncoder : IPayloadRequestEncoder<TOpCode, TArgs>
        => await Transport.SendAsync(TRequestEncoder.EncodeRequest(args), ct);

    /// <summary>
    /// Called to perform a handshake before the scanner connection is considered ready for use
    /// </summary>
    protected abstract Task Handshake(CancellationToken ct = default);
    
    public override async Task RunAsync(CancellationToken ct = default)
    {
        if (!Transport.IsOpen)
        {
            throw new InvalidOperationException("Transport is not open");
        }

        _bufferPipe = new Pipe();
        await this.Handshake(ct);
        
        //run dispatcher thread
        //so we dont spend valuable time handling events on the connection thread
        _ = DispatchAsync(ct);

        //now we read
        PipeReader reader = _bufferPipe.Reader;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                ReadResult res = await reader.ReadAsync(ct);
                ReadOnlySequence<byte> buffer = res.Buffer;

                // wait until we read everything
                while (Protocol.HandlePayload(ref buffer))
                {
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (res.IsCompleted) break;
            }
        }
        catch (OperationCanceledException)
        {
            //exit
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exception thrown in connection runner");
        }
        finally
        {
            await reader.CompleteAsync();
            if (Transport.IsOpen) await Transport.CloseAsync();
        }
    }

    public virtual async Task<Result<TResponse, Exception>> SendRequestAsync<TRequestEncoder, TArgs, TResponse>(
        TArgs args, CancellationToken ct = default)
        where TRequestEncoder : IPayloadRequestEncoder<TOpCode, TArgs>
        where TResponse : IScannerPayload
    {
        TOpCode opCode = TRequestEncoder.CommandDefinition.OpCode;
        TaskCompletionSource<IScannerPayload> source = new(TaskCreationOptions.RunContinuationsAsynchronously);

        await _writeSemaphore.WaitAsync(ct);
        try
        {
            lock (_commandQueues)
            {
                if (!_commandQueues.TryGetValue(opCode, out var queue))
                {
                    queue = new Queue<TaskCompletionSource<IScannerPayload>>();

                    _commandQueues[opCode] = queue;
                }

                queue.Enqueue(source);
            }

            await SendAsync<TRequestEncoder, TArgs>(args, ct);
        }
        finally
        {
            _writeSemaphore.Release();
        }

        try
        {
            return await Result<TResponse, Exception>.FromWrappedAsync(async () =>
                                                                           (TResponse)
                                                                           await source.Task
                                                                              .WaitAsync(TimeSpan.FromSeconds(5), ct)
                                                                      );
        }
        catch (TimeoutException ex)
        {
            return new Result<TResponse, Exception>(ex);
        }
        finally
        {
            lock (_commandQueues)
            {
                if (_commandQueues.TryGetValue(opCode, out var queue) && queue.Count > 0 && queue.Peek() == source)
                {
                    queue.Dequeue();
                }
            }
        }
    }

    public async Task StartAsync() => await this.Transport.OpenAsync();
    public async Task StopAsync() => await this.Transport.CloseAsync();

    private async Task DispatchAsync(CancellationToken ct)
    {
        try
        {
            await foreach (CommandPacketReceivedEventArgs<TOpCode> packet in _packetDispatcherQueue.Reader.ReadAllAsync(ct))
            {
                if (PayloadReceived.TryGetValue(packet.OpCode, out var ev))
                {
                    ev.Invoke(this, packet);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void OnTransportDataReceived(object? sender, ReadOnlyMemory<byte> buffer)
    {
        _bufferPipe.Writer.Write(buffer.Span);
        
        _ = _bufferPipe.Writer.FlushAsync().AsTask();
    }

    private void OnPacketReceived(object? sender, CommandPacketReceivedEventArgs<TOpCode> args)
    {
        if (!args.Payload.IsBroadcast)
        {
            lock (_commandQueues)
            {
                if (_commandQueues.TryGetValue(args.OpCode, out var queue) && queue.Count > 0)
                {
                    queue.Dequeue().SetResult(args.Payload);

                    return;
                }
            }
        }

        _packetDispatcherQueue.Writer.TryWrite(args);
    }
}