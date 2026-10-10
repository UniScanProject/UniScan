using System.Collections.Concurrent;
using System.Collections.Immutable;
using Serilog;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Util;
using UniScan.Device.Device;

namespace UniScan.Server.Core.Host;

public class DeviceHostManager
{
    private readonly ConcurrentDictionary<Slug<SnakeSlugFormatter>, DeviceHost> _devices = [];
    public IReadOnlyDictionary<Slug<SnakeSlugFormatter>, DeviceHost> Devices => _devices;

    public bool AddDevice(Slug<SnakeSlugFormatter> identifier, string? displayName, Device.Device.Device device)
    {
        try
        {
            var host = new DeviceHost(identifier, displayName, device);
            if (_devices.TryAdd(identifier, host))
            {
            }
            else
            {
                Log.Error("Attempted to add device with duplicate identifier '{Identifier}'", identifier);
            }

            return true;
        }
        catch (Exception exception)
        {
            return false;
        }
    }

    public bool AddDevice(Slug<SnakeSlugFormatter> identifier, Device.Device.Device device) =>
        AddDevice(identifier, null, device);

    public async Task StartAllAsync(CancellationToken ct = default)
    {
        var tasks = _devices.Values.Select(async host => 
        {
            try
            {
                await host.StartAsync(ct);
                // Log.Information("Info: {Model} v{Version}", host.Scanner.ScannerInfo?.Model, host.Scanner.ScannerInfo?.Version);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start device '{Device}'", host.DisplayString);
                await host.StopAsync();
            }
        });

        await Task.WhenAll(tasks);
    }
    
    public async Task DisconnectAllAsync()
    {
        await Task.WhenAll(_devices.Values.Where(x => x.Device.Active.Value).Select(x => x.StopAsync()));
        
        _devices.Clear();
    }
}