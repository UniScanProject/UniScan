using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Semver;
using Serilog;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Util;
using UniScan.Device.Device;
using UniScan.Server.Core.Host.Network;

namespace UniScan.Server.Core.Host;

public sealed class ScannerHost
{
    [JsonIgnore]
    public Slug<SnakeSlugFormatter> Identifier { get; } //when serialized, used as key in record instead
    
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; }

    [JsonPropertyName("scanner")]
    public Scanner Scanner { get; }
    
    [JsonIgnore]
    public ConnectionTask? ScannerTask { get; private set; }
    
    [JsonIgnore]
    public ILogger Logger { get; }

    [JsonIgnore] public HostClientsHandler NetworkClients { get; }
    
    public string DisplayString => DisplayName != null ? $"{DisplayName} ({Identifier})" : Identifier.ToString();
    
    public ScannerHost(Slug<SnakeSlugFormatter> identifier, Scanner scanner) : this(identifier, null, scanner) {}
    
    public ScannerHost(Slug<SnakeSlugFormatter> identifier, string? displayName, Scanner scanner)
    {
        Identifier = identifier;
        DisplayName = displayName;
        Scanner = scanner;

        this.Logger = Log.Logger.ForContext("SourceContext", ToString());
        
        NetworkClients = new HostClientsHandler(Identifier, Scanner);
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        Scanner.Connection.Transport.ConnectionStateChanged += (sender, connected) =>
        {
            if (!connected)
            {
                Logger.Information("Disconnected from scanner '{Scanner}' ({ScannerType})", DisplayString, Scanner);
                return;
            }
            
            Logger.Information("Connected to scanner '{ScannerName}' ({ScannerType})", DisplayString, Scanner);
        };

        try
        {
            Logger.Information("Connecting to scanner '{ScannerName}' ({ScannerType})", DisplayString, Scanner);
            await Scanner.ConnectAsync(ct);
        }
        catch (Exception ex)
        {
            throw new Exception($"Could not connect to scanner '{DisplayString}' ({Scanner})", ex);
        }

        CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(ct);
        ScannerTask = new ConnectionTask(Scanner.RunAsync(source.Token), source);
    }

    public async Task WaitAsync()
    {
        if (ScannerTask != null)
        {
            await ScannerTask.Task;
        }

        //await DisconnectAsync(); // task has finished, lets disconnect.
    }
    
    public async Task StopAsync()
    {
        Logger.Information("Waiting for scanner '{ScannerName}' ({ScannerType}) to disconnect...", DisplayString, Scanner);
        
        await Scanner.DisconnectAsync();
        
        if (ScannerTask != null)
        {
            await ScannerTask.TokenSource.CancelAsync();

            try
            {
                await ScannerTask.Task;
            }
            catch (OperationCanceledException)
            {
                /*good*/
            }
            finally
            {
                ScannerTask.TokenSource.Dispose();
                this.ScannerTask = null;
            }
        }
    }
    
    /// <inheritdoc/>
    public override string ToString() => DisplayName != null ? $"Scanner host for {DisplayName} ({Identifier})" : $"Scanner host for {Identifier}";
}