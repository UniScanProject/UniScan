using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using R3;
using Serilog;
using Shiki.Common.Identity;
using Shiki.Common.Factory;
using Shiki.ModuleManagement;
using Shiki.ModuleManagement.Implementations.Sources;
using UniScan.Client.Core.Config;
using UniScan.Client.Core.Config.Remote;
using UniScan.Client.Core.DI.Factory;
using UniScan.Client.Core.Module;
using UniScan.Client.Core.Module.Modules.Internal;
using UniScan.Client.Core.Remote;
using UniScan.Client.Core.Storage;
using UniScan.Client.Core.Storage.Serializer;
using UniScan.Core.Serialization;
using UniScan.Network;
using UniScan.Network.Data.Info.Software;
using UniScan.Network.Registry.Source.Sources;
using UniScan.Network.Socket.Configuration;
using UniScan.Platform;
using Constants = UniScan.Core.Constants;

namespace UniScan.Client.Core;

public partial class UniScanClient(
    IRemoteStorage remoteStorage
) : IDisposable
{
    private readonly IRemoteStorage _remoteStorage = remoteStorage;
    
    public static readonly Identifier ClientIdentifier = Constants.IdentifierNamespace.Derived("client");

    public void Dispose()
    {
        _remoteStorage.Dispose();
    }
}