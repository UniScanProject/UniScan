using System.Reflection;
using DotNetty.Transport.Channels;
using UniScan.Network.User.Permission;

namespace UniScan.Network.Server.Handler.Filter;

public class UserPermissionFilter : ChannelHandlerAdapter
{
    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        if (message is IPacket packet)
        {
            RequiredHandlerPermissionAttribute? attribute =
                message.GetType().GetCustomAttribute<RequiredHandlerPermissionAttribute>();

            if (attribute != null)
            {
                // TODO set up database of users, with permissions per scanner
                // users may have global permissions too, which we will check for first, and then we will check if packet has scanner instance interface implemented otherwise
            }
        }
        
        context.FireChannelRead(message);
    }
}