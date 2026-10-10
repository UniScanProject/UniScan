using System.Text.Json.Serialization;
using Serilog;
using UniScan.Device.Connection.Transport;

namespace UniScan.Device.Connection;

public interface IDeviceConnection
{
    [JsonInclude]
    [JsonPropertyName("method")]
    public ITransport Transport { get; }

    [JsonIgnore] protected ILogger Logger { get; }

    public Task StartAsync();
    public Task StopAsync();

    public Task RunAsync(CancellationToken ct = default);
}

public interface IDeviceConnection<TData> : IDeviceConnection
{
    new ITransport<TData> Transport { get; }
}