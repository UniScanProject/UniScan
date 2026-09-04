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

public interface IScannerEvents
{
}

public interface IScannerAPI
{
    public Task<string> GetModelAsync();
    public Task<SemVersion> GetVersionAsync();
}

public interface IScanner : IScannerAPI
{
    bool Active { get; }
    
    public IScannerConnection Connection { get; }
    public ITransport Transport { get; }
}

public record ScannerInfo(string Model, SemVersion Version);

public abstract class Scanner : IScanner, IScannerEvents
{
    [JsonPropertyName("$schema")]
    public const string Schema = "scanner.schema.json";

    [JsonIgnore]
    public IScannerConnection Connection { get; }
    
    [JsonPropertyName("transport")]
    [JsonInclude]
    public ITransport Transport => Connection.Transport;

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(State))]
    [MemberNotNullWhen(true, nameof(ScannerInfo))]
    public abstract bool Active { get; }
    
    [JsonIgnore]
    public abstract IReadOnlyBindableReactiveProperty<DeviceState?> State { get; }
    
    [JsonIgnore]
    public abstract ScannerInfo? ScannerInfo { get; protected set; }
    
    protected readonly ILogger Logger;
    
    public Scanner(IScannerConnection connection)
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

public abstract class Scanner<TConnection>(TConnection connection)
    : Scanner(connection)
    where TConnection : IScannerConnection
{
    [JsonIgnore]
    public new TConnection Connection => (TConnection)base.Connection;
}