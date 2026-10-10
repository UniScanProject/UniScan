using System.Numerics;
using DotNetty.Transport.Channels;
using R3;
using Shiki.Common.Extensions;
using Shiki.Common.Identity;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Internationalization;
using Shiki.Internationalization.Component;
using UniScan.Network;
using UniScan.Network.Protocol.Packets.Clientbound.SSR;
using UniScan.Server.Core.Host;
using UniScan.Server.Core.UserInterface.Providers;
using UniScan.UserInterface;

namespace UniScan.Server.Core.UserInterface;

public class DeviceSSRContextSession : IUISSRContextSession
{
    public Identifier ContextIdentifier { get; }

    private readonly CompositeDisposable _disposables = new();

    private readonly IChannel _channel;
    private readonly DeviceHost _device;
    private readonly Identifier _slotId;
    private readonly IUINodeProvider<DeviceLayoutContext> _nodeProvider;

    public DeviceSSRContextSession(IChannel channel, Identifier contextId, DeviceHost device, Identifier slotId,
                                   IEnumerable<IUINodeProvider> providers)
    {
        _channel = channel;
        _device = device;
        _slotId = slotId;
        ContextIdentifier = contextId;

        _nodeProvider =
            (providers.OrderByDescending(p => p.Priority).FirstOrDefault(p => p.Type == typeof(DeviceLayoutContext)) as
                 IUINodeProvider<DeviceLayoutContext>) ??
            throw new Exception("No node provider able to handle this device session");
    }

    private static class Identifiers
    {
        // public static readonly Identifier ZoneBindingIdentifier()
        public static readonly Identifier VolumePropertyIdentifier =
            new("UniScan", "device", "state", "property", "volume");

        public static readonly Identifier SquelchPropertyIdentifier =
            new("UniScan", "device", "state", "property", "squelch");
    }

    public Task InitializeAsync()
    {
        //init
        _channel.WriteAndFlushAsync(new SetUISlotPacket(_slotId, _nodeProvider.Provide(new DeviceLayoutContext(_device, _device.Device.State.Value))));

        //if possible, lets handle updates
        if (_nodeProvider is IUINetworkNodeUpdater<DeviceLayoutContext> updater)
        {
            _device.Device.State.AsObservable().DistinctUntilChanged().Subscribe((s) =>
            {
                updater.UpdateAsync(_channel, new DeviceLayoutContext(_device, s));
            }).AddTo(_disposables);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}