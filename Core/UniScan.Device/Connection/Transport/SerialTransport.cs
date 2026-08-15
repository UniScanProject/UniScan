using System.Buffers;
using System.IO.Ports;
using System.Text.Json.Serialization;
using Shiki.Common.Serialization.Polymorphism;

namespace UniScan.Device.Connection.Transport;

[PolymorphicSerializable<ITransport>("Serial")]
public class SerialTransport : ITransport<ReadOnlyMemory<byte>>
{
    [JsonPropertyName("port")]
    [JsonInclude]
    public string Port { get; }

    [JsonPropertyName("baud_rate")]
    [JsonInclude]
    public int BaudRate { get; init; } = 115200;

    [JsonIgnore]
    private SerialPort? _port;
    [JsonIgnore] private CancellationTokenSource? _portCts;

    [JsonIgnore]
    public bool IsOpen => _port is { IsOpen: true };

    public SerialTransport(string port)
    {
        this.Port = port;
    }

    public Task OpenAsync(CancellationToken ct = default)
    {
        if (IsOpen) return Task.CompletedTask;
        
        _port = new SerialPort(Port, BaudRate)
        {
            ReadTimeout = 500,
            WriteTimeout = 500,
            DtrEnable = true,
            RtsEnable = true,
        };
        
        _port.Open();
        ConnectionStateChanged?.Invoke(this, true);
        
        _portCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _ = RunReadAsync(_portCts.Token);

        return Task.CompletedTask;
    }

    private async Task RunReadAsync(CancellationToken ct = default)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    int read = await _port!.BaseStream.ReadAsync(buffer, ct);
                    if (read > 0)
                    {
                        DataReceived?.Invoke(this, buffer.AsMemory(0, read));
                    }
                }
                catch (TimeoutException ex)
                {
                    continue;
                }
            }
        }
        catch (OperationCanceledException ex)
        {
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, ex);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);

            if (IsOpen)
                _ = CloseAsync();
        }
    }
    
    public Task CloseAsync()
    {
        ConnectionStateChanged?.Invoke(this, false);

        _portCts?.Cancel();
        if (IsOpen)
        {
            _port!.BaseStream.Close();
            _port.Close();
        }
        
        return Task.CompletedTask;
    }

    public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default)
    {
        if (!IsOpen) throw new InvalidOperationException("Serial port is not open");
        
        await _port!.BaseStream.WriteAsync(buffer, ct);
    }

    public async ValueTask DisposeAsync() => await CloseAsync();
    public void Dispose() => this._port?.Dispose();
    
    public event EventHandler<bool>? ConnectionStateChanged;
    public event EventHandler<ReadOnlyMemory<byte>>? DataReceived;
    public event EventHandler<Exception>? Error;
}