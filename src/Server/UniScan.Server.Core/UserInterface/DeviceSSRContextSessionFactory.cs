using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;
using Shiki.Common.Identity;
using UniScan.Network.Data.SSR;
using UniScan.Server.Core.Host;

namespace UniScan.Server.Core.UserInterface;

public class DeviceSSRContextSessionFactory(DeviceHostManager manager, IServiceProvider provider) : IUISSRContextSessionFactory<DeviceUIContextSubscribeArgs>
{
    public Identifier ContextIdentifier => new("UniScan", "ssr", "context", "device");

    public IUISSRContextSession Create(IChannel channel, Identifier contextIdentifier, DeviceUIContextSubscribeArgs args)
    {
        if (!manager.Devices.TryGetValue(args.DeviceIdentifier, out DeviceHost? device))
        {
            throw new KeyNotFoundException("Device not found");
        }
        
        return new DeviceSSRContextSession(channel, contextIdentifier, device, args.SlotIdentifier, provider.GetServices<IUINodeProvider>());//todo if I want to avoid listening to the scanner tons of times lets cache existing instances by scanner and subscribe the channel to those instead. 
    }
}