using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using R3;
using Semver;
using Serilog;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Util;
using UniScan.Device.Device;
using UniScan.Server.Core.Host.Network;

namespace UniScan.Server.Core.Host;

public sealed class DeviceHost
{
    [JsonIgnore] public Slug<SnakeSlugFormatter> Identifier { get; } //when serialized, used as key in record instead

    [JsonPropertyName("displayName")] public string? DisplayName { get; }

    [JsonPropertyName("device")] public Device.Device.Device Device { get; }

    [JsonIgnore] public ConnectionTask? DeviceRunTask { get; private set; }

    [JsonIgnore] public ILogger Logger { get; }

    [JsonIgnore] public HostClientsHandler NetworkClients { get; }

    public string DisplayString => DisplayName != null ? $"{DisplayName} ({Identifier})" : Identifier.ToString();

    public DeviceHost(Slug<SnakeSlugFormatter> identifier, Device.Device.Device device) : this(identifier, null, device)
    {
    }

    public DeviceHost(Slug<SnakeSlugFormatter> identifier, string? displayName, Device.Device.Device device)
    {
        Identifier = identifier;
        DisplayName = displayName;
        Device = device;

        this.Logger = Log.ForContext("SourceContext", ToString());

        NetworkClients = new HostClientsHandler(Identifier, Device);
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        Device.Connection.Transport.IsOpen.AsObservable().Subscribe((connected) => {
            if (!connected)
            {
                Logger.Information("Disconnected from device '{Device}' ({DeviceType})", DisplayString, Device);
                return;
            }

            Logger.Information("Connected to device '{DeviceName}' ({DeviceType})", DisplayString, Device);
        });

        try
        {
            Logger.Information("Connecting to device '{DeviceName}' ({DeviceType})", DisplayString, Device);
            await Device.ConnectAsync(ct);
        }
        catch (Exception ex)
        {
            throw new Exception($"Could not connect to device '{DisplayString}' ({Device})", ex);
        }

        CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(ct);
        DeviceRunTask = new ConnectionTask(Device.RunAsync(source.Token), source);
    }

    public async Task WaitAsync()
    {
        if (DeviceRunTask != null)
        {
            await DeviceRunTask.Task;
        }

        //await DisconnectAsync(); // task has finished, lets disconnect.
    }

    public async Task StopAsync()
    {
        Logger.Information("Waiting for device '{DeviceName}' ({DeviceType}) to disconnect...", DisplayString, Device);

        await Device.DisconnectAsync();

        if (DeviceRunTask != null)
        {
            await DeviceRunTask.TokenSource.CancelAsync();

            try
            {
                await DeviceRunTask.Task;
            }
            catch (OperationCanceledException)
            {
                /*good*/
            }
            finally
            {
                DeviceRunTask.TokenSource.Dispose();
                this.DeviceRunTask = null;
            }
        }
    }

    /// <inheritdoc/>
    public override string ToString() => DisplayName != null
                                             ? $"Device host for {DisplayName} ({Identifier})"
                                             : $"Device host for {Identifier}";
}