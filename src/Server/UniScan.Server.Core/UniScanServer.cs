using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Semver;
using Serilog;
using Serilog.Core;
using Shiki.Common.Extensions;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Common.Util;
using Shiki.FileSystem.Implementations.Assembly;
using Shiki.FileSystem.Storage;
using Shiki.FileSystem.Storage.Directory.KeyValue;
using Shiki.FileSystem.Storage.Serializer;
using Shiki.Internationalization;
using Shiki.Internationalization.Component;
using Shiki.Internationalization.Component.Resolver;
using Shiki.Internationalization.Translation;
using Shiki.Internationalization.Translation.LanguageSource;
using Shiki.ModuleManagement;
using Shiki.TaskPipeline.Context;
using Shiki.Settings;
using Shiki.Settings.Option;
using UniScan.Core.State;
using UniScan.Device.Device;
using UniScan.Network;
using UniScan.Network.Data.Info.Remote;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Group;
using UniScan.Network.Protocol.Packets.Bidirectional.Status;
using UniScan.Network.Registry;
using UniScan.Network.Registry.Source.Sources;
using UniScan.Network.Server;
using UniScan.Network.Server.HostMethod;
using UniScan.Network.Socket;
using UniScan.Network.Socket.Configuration;
using UniScan.Server.Authentication.Session;
using UniScan.Server.Core.Host;
using UniScan.Server.Core.Module;
using UniScan.Server.Core.Pipeline;
using UniScan.Server.Core.Social;
using UniScan.Server.Core.UserInterface;
using Constants = UniScan.Core.Constants;

namespace UniScan.Server.Core;

public class MainOptionGroup : IIdentifiableOptionGroup
{
    public static Identifier Identifier { get; } = UniScanServer.Identifier.Derived("settings", "group", "main");
    
    //TODO I can codegen this like avalonia
    [JsonIgnore]
    public SettingOption<List<IServerHostMethod>> HostMethodsOption { get; } = new(
                                                                             UniScanServer.Identifier.Derived("settings", "option",
                                                                                 "host_methods"),
                                                                             Constants.Settings.MainCategory,
                                                                             []
                                                                            );

    [JsonIgnore]
    public SettingOption<bool> AllowAnonymousLoginOption { get; } = new(UniScanServer.Identifier.Derived("settings",
                                                                            "option",
                                                                            "allow_anonymous_login"),
                                                                        Constants.Settings.MainCategory,
                                                                        true);
    
    [JsonIgnore]
    public SettingOption<string> NetworkGroupMethodOption { get; } = new(UniScanServer.Identifier.Derived("settings",
                                                                            "option",
                                                                            "network_group_method"),
                                                                        Constants.Settings.MainCategory,
                                                                        MultithreadedNetworkGroupManager.Id);

    [JsonPropertyName("allow_anonymous_login")]
    public bool AllowAnonymousLogin
    {
        get => AllowAnonymousLoginOption.Value;
        set => AllowAnonymousLoginOption.SetValue(value);
    }
    
    [JsonPropertyName("network_group_method")]
    public string NetworkGroupMethod
    {
        get => NetworkGroupMethodOption.Value;
        set => NetworkGroupMethodOption.SetValue(value);
    }
    
    [JsonPropertyName("host_methods")]
    public List<IServerHostMethod> HostMethods
    {
        get => HostMethodsOption.Value;
        set => HostMethodsOption.SetValue(value);
    }
    
    public IEnumerable<IOption> GetOptions()
    {
        yield return NetworkGroupMethodOption;
        yield return AllowAnonymousLoginOption;
        yield return HostMethodsOption;
    }
}

public partial class UniScanServer
{
    public static readonly Identifier Identifier = UniScan.Core.Constants.IdentifierNamespace.Derived("server");
    public static readonly ServerSoftwareInfo SoftwareInfo = new(
        new SoftwareAssemblyInfo(Identifier, SemVersion.Parse(Assembly.GetCallingAssembly().InformationalVersionString!)),
        Network.Constants.ProtocolVersion,
        "UniScan Server",
        "https://github.com/UniScanProject/UniScan"
    );
    
    public string RootDirectory { get; private set; }
    public IServiceProvider ServiceProvider { get; private set; }

    //modules
    private ModuleStorage<IUniScanServerModule, UniScanServerModuleInitializationArgs> _moduleStorage = null!;

    //management
    public SessionManager SessionManager { get; private set; }
    public DeviceHostManager DeviceManager { get; private set; }
    public RemoteInfoManager RemoteInfoManager { get; private set; }
    public SettingsManager Settings { get; private set; }
    
    //networking
    public ISocket Socket { get; private set; }
    private UniScanServerChannelInitializer _channelInitializer;
    private PacketRegistry _packetRegistry;
    
    
    public UniScanServer(string rootDirectory)
    {
        RootDirectory = rootDirectory;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        //start scanners
        Log.Information("Connecting to devices");
        await DeviceManager.StartAllAsync(ct);
        
        Log.Information("Starting socket");
        await Socket.StartAsync();

        try
        {
            Log.Information("Server is running");

            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (TaskCanceledException)
        {
        }
    }

    public async Task ExitAsync()
    {
        Log.Information("Closing socket...");
        if (Socket is ServerSocket serverSocket)
        {
            await serverSocket.ClientManager.BroadcastAsync(new DisconnectPacket(new TranslatableTextComponent("UniScan:i18n/disconnect/server_shutting_down")));
        }

        await Socket.StopAsync();

        Log.Information("Disconnecting devices...");
        await DeviceManager.DisconnectAllAsync();
    }
}