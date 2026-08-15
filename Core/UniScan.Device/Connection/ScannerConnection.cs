using Serilog;
using UniScan.Device.Connection.Protocol;
using UniScan.Device.Connection.Protocol.Payload;
using UniScan.Device.Connection.Transport;

namespace UniScan.Device.Connection;

public abstract class ScannerConnection<TData>(ITransport<TData> transport) : IScannerConnection<TData>
{
    public ITransport<TData> Transport { get; } = transport;
    ITransport IScannerConnection.Transport => Transport;

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