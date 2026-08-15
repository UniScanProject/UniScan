using System.Text.Json.Serialization;

namespace UniScan.Device.Connection.Transport;

/// <summary>
/// Connection method, used to connect and send data to a Scanner
/// </summary>
public interface ITransport : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Whether the connection is open
    /// </summary>
    [JsonIgnore]
    public bool IsOpen { get; }

    /// <summary>
    /// Opens the connection
    /// </summary>
    public Task OpenAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Closes the connection
    /// </summary>
    public Task CloseAsync();
    
    /// <summary>
    /// Fired when the connection state changes
    /// </summary>
    public event EventHandler<bool>? ConnectionStateChanged;
    
    /// <summary>
    /// Fired when an error was encountered in transport
    /// </summary>
    public event EventHandler<Exception>? Error;
}

public interface ITransport<TData> : ITransport
{
    /// <summary>
    /// Fired when the there is data ready to read
    /// </summary>
    public event EventHandler<TData>? DataReceived;
    
    public ValueTask SendAsync(TData data, CancellationToken ct = default);

}