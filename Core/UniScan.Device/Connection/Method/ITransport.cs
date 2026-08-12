using System.Text.Json.Serialization;

namespace UniScan.Device.Connection.Method;

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

    public ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default);
    
    /// <summary>
    /// Fired when the connection state changes
    /// </summary>
    public event EventHandler<bool>? ConnectionStateChanged;
    
    /// <summary>
    /// Fired when the there is data ready to read
    /// </summary>
    public event EventHandler<ReadOnlyMemory<byte>>? DataReceived;
    
    /// <summary>
    /// Fired when an error was encountered in transport
    /// </summary>
    public event EventHandler<Exception>? Error;
}