using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using UniScan.Network.Client.Remote.Connection;
using UniScan.Network.Client.Remote.Connection.Methods;

namespace UniScan.Network.Client.Extensions;

public static class BootstrapExtensions
{
    extension(Bootstrap bootstrap)
    {
        public Bootstrap ConnectionMethod(IRemoteConnectionMethod method)
        {
            method.Apply(bootstrap);
            return bootstrap;
        }
    }
}