using System.Collections.Concurrent;
using System.Collections.Immutable;
using Serilog;
using Shiki.Common.Identity;
using Shiki.Common.Util;
using UniScan.Device.Device;

namespace UniScan.Server.Core.Host;

public class ScannerHostManager
{
    private readonly ConcurrentDictionary<Identifier, ScannerHost> _scanners = [];
    public IReadOnlyDictionary<Identifier, ScannerHost> Scanners => _scanners;

    public bool AddScanner(Identifier identifier, string? displayName, Scanner scanner)
    {
        try
        {
            var host = new ScannerHost(identifier, displayName, scanner);
            if (_scanners.TryAdd(identifier, host))
            {
            }
            else
            {
                Log.Logger.Error("Attempted to add scanner with duplicate identifier '{Identifier}'", identifier);
            }

            return true;
        }
        catch (Exception exception)
        {
            return false;
        }
    }

    public bool AddScanner(Identifier identifier, Scanner scanner) =>
        AddScanner(identifier, null, scanner);

    public async Task StartAllAsync(CancellationToken ct = default)
    {
        var tasks = _scanners.Values.Select(async host => 
        {
            try
            {
                await host.StartAsync(ct);
                // Log.Information("Info: {Model} v{Version}", host.Scanner.ScannerInfo?.Model, host.Scanner.ScannerInfo?.Version);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Failed to start scanner '{Scanner}'", host.DisplayString);
                await host.StopAsync();
            }
        });

        await Task.WhenAll(tasks);
    }
    
    public async Task DisconnectAllAsync()
    {
        await Task.WhenAll(_scanners.Values.Where(x => x.Scanner.Active).Select(x => x.StopAsync()));
        
        _scanners.Clear();
    }
}