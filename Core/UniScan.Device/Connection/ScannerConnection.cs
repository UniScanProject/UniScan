using Serilog;
using UniScan.Device.Connection.Protocol;
using UniScan.Device.Connection.Protocol.Payload;
using UniScan.Device.Connection.Transport;

namespace UniScan.Device.Connection;

public abstract class ScannerConnection<TData, TOutputPayload>(ITransport<TData> transport) : IScannerConnection<TData>
{
    public ITransport<TData> Transport { get; } = transport;
    ITransport IScannerConnection.Transport => Transport;

    public ILogger Logger { get; }

    public event EventHandler<PacketReceivedEventArgs<TOutputPayload>>? PacketReceived;

    public async Task StartAsync() => await Transport.OpenAsync();

    public async Task StopAsync() => await Transport.CloseAsync();
    
    protected virtual void OnPacketReceived(TOutputPayload payload)
    {
        PacketReceived?.Invoke(this, new PacketReceivedEventArgs<TOutputPayload>(payload));
    }

    public abstract Task RunAsync(CancellationToken ct = default);
}