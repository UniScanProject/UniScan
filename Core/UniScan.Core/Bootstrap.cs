using System.Runtime.CompilerServices;
using MessagePack;
using MessagePack.Resolvers;
using Shiki.Extensions.MessagePack.Formatter.Identity;
using Shiki.Extensions.MessagePack.Formatter.Identity.Slug;

namespace UniScan.Core;

public static class Bootstrap
{
    [ModuleInitializer]
    public static void InitializeMessagePack()
    {
        var resolver = CompositeResolver.Create(
                                                SlugFormatterResolver.Instance,
                                                StandardResolver.Instance
                                               );
                                                
        var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
        
        MessagePackSerializer.DefaultOptions = options;
    }
}