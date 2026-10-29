using System.Runtime.CompilerServices;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Client.Remote.Connection.Methods;
using UniScan.Network.Group;

namespace UniScan.Network.Client.Extensions;

public static class BootstrapExtensions
{
    extension(Bootstrap bootstrap)
    {
        public Bootstrap ConnectionMethod(IRemoteConnectionMethod method, INetworkGroupManager networkGroupManager)
        {
            method.Apply(bootstrap, networkGroupManager);
            return bootstrap;
        }
    }
}