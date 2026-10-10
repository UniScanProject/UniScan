using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using R3;
using Semver;
using Serilog;
using UniScan.Core.State;
using UniScan.Core.State.Display;
using UniScan.Device.Connection;
using UniScan.Device.Connection.Command;
using UniScan.Device.Connection.Transport;

namespace UniScan.Device.Device;


public interface IDeviceAPI
{
    public Task<string> GetModelAsync();
    public Task<SemVersion> GetVersionAsync();
}

public interface IDevice : IDeviceAPI
{
    IReadOnlyBindableReactiveProperty<bool> Active { get; }
    IReadOnlyBindableReactiveProperty<IDeviceState?> State { get; }
    
    public IDeviceConnection Connection { get; }
    public ITransport Transport { get; }
}

public record DeviceInfo(string Model, SemVersion Version);

public abstract class Device : IDevice
{
    [JsonPropertyName("$schema")]
    public const string Schema = "device.schema.json";

    [JsonIgnore]
    public IDeviceConnection Connection { get; }
    
    [JsonPropertyName("transport")]
    [JsonInclude]
    public ITransport Transport => Connection.Transport;

    [JsonIgnore]
    public abstract IReadOnlyBindableReactiveProperty<bool> Active { get; }
    
    [JsonIgnore]
    public abstract IReadOnlyBindableReactiveProperty<IDeviceState?> State { get; }
    
    [JsonIgnore]
    public abstract DeviceInfo? DeviceInfo { get; protected set; }
    
    protected readonly ILogger Logger;
    
    public Device(IDeviceConnection connection)
    {
        this.Logger = Log.ForContext("SourceContext", this.GetType().Name);

        this.Connection = connection;

        // this.Connection.SetBaseLogger(this.Logger);
    }
    
    public abstract Task<string> GetModelAsync();
    public abstract Task<SemVersion> GetVersionAsync();

    public virtual async Task ConnectAsync(CancellationToken ct = default) => await this.Connection.StartAsync();
    public abstract Task RunAsync(CancellationToken ct = default);
    public virtual async Task DisconnectAsync(CancellationToken ct = default) => await this.Connection.StopAsync();
}

public abstract class Device<TConnection>(TConnection connection)
    : Device(connection)
    where TConnection : IDeviceConnection
{
    [JsonIgnore]
    public new TConnection Connection => (TConnection)base.Connection;
}