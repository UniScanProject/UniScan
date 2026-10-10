using DotNetty.Transport.Channels;
using Shiki.Common.Extensions;
using Shiki.Common.Identity.Slug;
using Shiki.Common.Identity.Slug.Formatting.Formatters;
using Shiki.Internationalization.Translation;
using UniScan.Client.Core.Remote;
using UniScan.Network.Protocol.Packets.Clientbound.Internationalization;

namespace UniScan.Client.Core.Module.Modules.Internal.Handler;

public class LanguageStringsPacketHandler(TranslationManager manager) : SimpleChannelInboundHandler<LanguageStringsPacket>
{
    protected override void ChannelRead0(IChannelHandlerContext ctx, LanguageStringsPacket msg)
    {
        if (!manager.Languages.TryGetValue(msg.Id, out Language? language)) return;

        RemoteServer server = ctx.Channel.GetAttribute(ServerAttributes.ServerAttribute).Get();

        var overlayId = $"server_{server.Id:N}".ToSlug<SnakeSlugFormatter>();
        if (language.GetMutableOverlay(overlayId) is not { } overlay)
        {
            language.SetOverlay(overlayId, 5, msg.Strings);
            return;
        }
        
        foreach (var str in msg.Strings)
        {
            if (str.Value == null)
            {
                overlay.MutableStrings.Remove(str.Key);
                continue;
            }
            
            overlay.MutableStrings.TryAdd(str.Key, str.Value);
        }
    }
}